using Humanizer;
using MaHakesherServerSide.Data;
using MaHakesherServerSide.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;

namespace MaHakesherServerSide.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ConnectionsController : Controller
    {
        private readonly MySqlConnection _connection;
        private readonly int MAXIMUM_FAILURES = 4;
        private readonly string NAME_OF_GOD = "Yhvh_1";

        public ConnectionsController(MySqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<string>?> GetBooksPersonAppear(string name)
        {
            List<string> booksList = new List<string>();
            try
            {
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"USE mahakesher; " +
                    $"SELECT DISTINCT  r.usx_code " +
                    $"FROM mahakesher.person_verse p " +
                    $"JOIN mahakesher.bibledata_reference r ON p.reference_id = r.reference_id " +
                    $"WHERE p.person_id IN( " +
                    $"SELECT person_id " +
                    $"FROM mahakesher.person_verse " +
                    $"WHERE person_id = '{name}' );", _connection);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var value = reader.GetValue(0);
                    booksList.Add(value.ToString());
                }
                await _connection.CloseAsync();
                return booksList;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return null;
        }

        public async Task<Dictionary<string, string>?> GetRelations(string name)
        {
            Dictionary<string, string> relationsDictionary = new Dictionary<string, string>();
            try
            {
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"USE mahakesher; " +
                    $"SELECT person_id_1 AS other_person, relationship_type " +
                    $"FROM mahakesher.relationship " +
                    $"WHERE person_id_2 ='{name}';", _connection);
                using var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    string personId2 = reader["other_person"].ToString();
                    string relationshipType = reader["relationship_type"].ToString();

                    relationsDictionary.Add(personId2, relationshipType);
                }
                await _connection.CloseAsync();
                return relationsDictionary;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return null;
        }

        public async Task<List<string>?> GetPeopleThatMentionsInPersonLifeTime(string person)
        {
            List<string> personsList = new List<string>();
            try
            {
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"WITH NameReference AS ( " +
                                                     $"  SELECT DISTINCT " +
                                                     $"    pv.person_id, " +
                                                     $"    MIN( " +
                                                     $"      (SELECT verse_sequence " +
                                                     $"      FROM mahakesher.bibledata_reference " +
                                                     $"      WHERE reference_id = e.start_year_reference_id) " +
                                                     $"    ) AS start_value, " +
                                                     $"    MAX( " +
                                                     $"      (SELECT verse_sequence " +
                                                     $"      FROM mahakesher.bibledata_reference " +
                                                     $"      WHERE reference_id = e.period_length_reference_id) " +
                                                     $"    ) AS end_value" +
                                                     $"  FROM mahakesher.epoch e" +
                                                     $"  JOIN mahakesher.person_verse pv ON e.person_id = pv.person_id" +
                                                     $"  WHERE pv.person_id = '{person}' " +
                                                     $"  GROUP BY pv.person_id) " +
                                                     $", ReferenceRange AS( " +
                                                     $"  SELECT reference_id" +
                                                     $"  FROM mahakesher.bibledata_reference " +
                                                     $"  WHERE verse_sequence BETWEEN (SELECT start_value FROM NameReference) AND (SELECT end_value FROM NameReference) " +
                                                     $"  )" +
                                                     $"SELECT DISTINCT pv.person_id " +
                                                     $"FROM mahakesher.person_verse pv " +
                                                     $"INNER JOIN ReferenceRange rr ON pv.reference_id = rr.reference_id " +
                                                     $"WHERE pv.person_id<> '{person}'; ", _connection);
                using var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var value = reader.GetValue(0);
                    personsList.Add(value.ToString());
                }
                await _connection.CloseAsync();
                return personsList;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return null;
        }

        public async Task<Dictionary<string, string>?> GetPeopleThatMentionInTheSameVerse(string person)
        {
            Dictionary<string, string> relationsDictionary = new Dictionary<string, string>();
            try
            {
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"SELECT person_id, " +
                                                     $"GROUP_CONCAT(DISTINCT reference_id ORDER BY reference_id ASC SEPARATOR ', ') AS concatenated_reference_ids " +
                                                     $"FROM mahakesher.person_verse " +
                                                     $"WHERE reference_id IN( " +
                                                     $"SELECT reference_id " +
                                                     $"FROM mahakesher.person_verse " +
                                                     $"WHERE person_id = 'Hezron_2') " +
                                                     $"AND person_id<> 'Hezron_2' " +
                                                     $"AND person_id<> 'Yhvh_1' " +
                                                     $"GROUP BY person_id;", _connection);
                using var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    string personId = reader["person_id"].ToString();
                    string verses = reader["concatenated_reference_ids"].ToString();

                    relationsDictionary.Add(personId, verses);
                }
                await _connection.CloseAsync();
                return relationsDictionary;
            }
            catch (Exception ex)
            {
                await _connection.CloseAsync();
                Console.Write(ex.Message);
            }
            return null;
        }

        public async Task<Dictionary<string, string>> GetPlayRoad([FromQuery] int difficultyLevel)

        {
            if (difficultyLevel < 2) difficultyLevel = 2;

            Dictionary<string, string> playDictionary = new Dictionary<string, string>();
            string person = await GetRandomPersonId();
            playDictionary.Add(person, "Start");

            List<string> personsInRoad = new List<string>(); 
            personsInRoad.Add(person);

            string nextPerson = person;
            int failed = 0;
            for (int i = 0; i < difficultyLevel; i++)
            {
                KeyValuePair<string, string>? keyValuePair = await NextLevel(nextPerson);
                if (keyValuePair == null)
                {
                    i--;
                    failed++;
                    if (failed > MAXIMUM_FAILURES)
                    {
                        return await GetPlayRoad(difficultyLevel - 1);
                    }
                } else
                {
                    string optionPerson = keyValuePair.Value.Key;
                    if (personsInRoad.Contains(optionPerson))
                    {
                        i--;
                        failed++;
                        continue;
                    }
                    personsInRoad.Add(optionPerson);
                    playDictionary.Add(optionPerson, keyValuePair.Value.Value);
                    nextPerson = optionPerson;
                }
            }
            return playDictionary;
        }

        private async Task<KeyValuePair<string, string>?> NextLevel(string personId)
        {
            Random random = new Random();
            int randNumber = random.Next(3);
            switch (randNumber)
            {
                case 0:
                    {
                        Dictionary<string, string>? relations = await GetRelations(personId);
                        if (relations == null || relations.Count == 0)
                        {
                            return null;
                        }
                        KeyValuePair<string, string> relation = relations.ElementAt(random.Next(0, relations.Count));
                        return new KeyValuePair<string, string>(relation.Key, $"The {relation.Value} of {personId}");
                    }
                case 1:
                    {
                        List<string>? mentionInPersonLifeTime = await GetPeopleThatMentionsInPersonLifeTime(personId);
                        if (mentionInPersonLifeTime == null || mentionInPersonLifeTime.Count == 0)
                        {
                            return null;
                        }
                        string person = mentionInPersonLifeTime.ElementAt(random.Next(0, mentionInPersonLifeTime.Count));
                        string primaryPersonName = await GetNameFromPersonId(personId);
                        return new KeyValuePair<string, string>(person, $"Mentioned in {primaryPersonName} life time");
                    }
                case 2:
                    {
                        Dictionary<string, string>? mentionInSameVerse = await GetPeopleThatMentionInTheSameVerse(personId);
                        if (mentionInSameVerse == null || mentionInSameVerse.Count == 0)
                        {
                            return null;
                        }
                        KeyValuePair<string,string> relation = mentionInSameVerse.ElementAt(random.Next(0, mentionInSameVerse.Count));
                        return new KeyValuePair<string, string>(relation.Key, $"Mentioned together in verses: {relation.Value}");
                    }
                default: { return null; }
            }
        }


        private async Task<string> GetNameFromPersonId(string personId)
        {
            string? personName = null;
            try
            {
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"SELECT person_name" +
                    $"FROM mahakesher.person" +
                    $"WHERE person_name = {personId}", _connection);
                using var reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    var value = reader.GetValue(0);
                    personName = value.ToString();
                }
                await _connection.CloseAsync();
                if (string.IsNullOrEmpty(personName))
                {
                    throw new Exception($"Problem in getting person name for {personId}");
                }
                return personName;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return string.Empty;
        }

        private async Task<string> GetRandomPersonId()
        {
            string? person = await RandomRawAsync("mahakesher.person", "person_id");
            if (person == null)
            {
                throw new Exception("Problem in retrieving random person");
            } if (person == NAME_OF_GOD)
            {
                return await GetRandomPersonId();
            }
            return person;
        }

        private async Task<string?> RandomRawAsync(string tableName, string columnName)
        {
            try
            {
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"SELECT {columnName} FROM {tableName} ORDER BY RAND ( )  LIMIT 1;", _connection);
                using var reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    string value = reader.GetValue(0).ToString();
                    await _connection.CloseAsync();
                    return value;
                }

            }
            catch (Exception ex)
            {
                await _connection.CloseAsync();
                Console.Write(ex.Message);
            }
            return null;
        }

    }
}
