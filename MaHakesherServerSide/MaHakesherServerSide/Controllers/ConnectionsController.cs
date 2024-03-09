using Microsoft.AspNetCore.Mvc;
using MySqlConnector;


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
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        string personId2 = reader["other_person"].ToString();
                        string relationshipType = reader["relationship_type"].ToString();

                        relationsDictionary.Add(personId2, relationshipType);
                    }
                    if (relationsDictionary.ContainsKey(NAME_OF_GOD))
                    {
                        relationsDictionary.Remove(NAME_OF_GOD);
                    }
                }
                return relationsDictionary;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                await _connection.CloseAsync();
            }
            return null;
        }

        public async Task<List<string>?> GetPeopleThatMentionsInPersonLifeTime(string personId)
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
                                                     $"  WHERE pv.person_id = '{personId}' " +
                                                     $"  GROUP BY pv.person_id) " +
                                                     $", ReferenceRange AS( " +
                                                     $"  SELECT reference_id" +
                                                     $"  FROM mahakesher.bibledata_reference " +
                                                     $"  WHERE verse_sequence BETWEEN (SELECT start_value FROM NameReference) AND (SELECT end_value FROM NameReference) " +
                                                     $"  )" +
                                                     $"SELECT DISTINCT pv.person_id " +
                                                     $"FROM mahakesher.person_verse pv " +
                                                     $"INNER JOIN ReferenceRange rr ON pv.reference_id = rr.reference_id " +
                                                     $"WHERE pv.person_id<> '{personId}'; ", _connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var value = reader.GetValue(0);
                        personsList.Add(value.ToString());
                    }
                }
                return personsList;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                await _connection.CloseAsync();
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
                                                     $"WHERE person_id = '{person}') " +
                                                     $"AND person_id<> '{person}' " +
                                                     $"AND person_id<> '{NAME_OF_GOD}' " +
                                                     $"GROUP BY person_id;", _connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        string personId = reader["person_id"].ToString();
                        string verses = reader["concatenated_reference_ids"].ToString();

                        relationsDictionary.Add(personId, verses);
                    }
                }
                return relationsDictionary;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                await _connection.CloseAsync();
            }
            return null;
        }

        public async Task<Dictionary<string, string>> GetPlayRoad([FromQuery] int difficultyLevel)

        {
            if (difficultyLevel < 2) difficultyLevel = 2;

            Dictionary<string, string> playDictionary = new Dictionary<string, string>();
            string person = await GetRandomPersonId();
            playDictionary.Add(person, $"Start $$${await GetGenderFromPersonId(person)}");

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
                    playDictionary.Add(optionPerson, keyValuePair.Value.Value + $"$$${await GetGenderFromPersonId(optionPerson)}");
                    nextPerson = optionPerson;
                }
            }
            if (playDictionary.Count >= 6)
            {
                return playDictionary.Take(5).ToDictionary(pair=>pair.Key, pair=>pair.Value);
            }
            return playDictionary;
        }

        public async Task<Dictionary<string, string>> GetOptions([FromQuery] string personId)
        {
            Dictionary<string, string>? connections = new Dictionary<string, string>();
            string primaryPersonName = await GetNameFromPersonId(personId);
            Dictionary<string, string>? relations = await GetRelations(personId);
            if (relations != null)
            {
                foreach (KeyValuePair<string, string> person in relations)
                {
                    if (!connections.ContainsKey(person.Key))
                    {
                        connections.Add(person.Key, $"The {person.Value} of {primaryPersonName} $$${await GetGenderFromPersonId(person.Key)}");
                    }
                }
            }

            List<string>? mentionInPersonLifeTime = await GetPeopleThatMentionsInPersonLifeTime(personId);
            mentionInPersonLifeTime.ForEach(async (person) =>
            {
                if (!connections.ContainsKey(person))
                {
                    connections.Add(person, $"Mentioned in {primaryPersonName} life time $$${await GetGenderFromPersonId(person)}");
                }
            });
            
            Dictionary<string, string>? mentionInSameVerse = await GetPeopleThatMentionInTheSameVerse(personId);
            if (mentionInSameVerse != null)
            {
                    foreach (KeyValuePair<string, string> person in mentionInSameVerse)
                    {
                        if (!connections.ContainsKey(person.Key))
                        {
                            connections.Add(person.Key, $"Mentioned together in verses: {person.Value} $$${await GetGenderFromPersonId(person.Key)}");
                        }
                    }
            }

            if (connections.ContainsKey(NAME_OF_GOD))
            {
                connections.Remove(NAME_OF_GOD);
            }
            return connections;
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

        private async Task<string> GetGenderFromPersonId(string personId)
        {
            string? personGender = null;
            try
            {
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"SELECT sex " +
                   $"FROM mahakesher.person " +
                   $"WHERE person_id = '{personId}'", _connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        var value = reader.GetValue(0);
                        personGender = value.ToString();
                    }
                }
                if (string.IsNullOrEmpty(personGender))
                {
                    throw new Exception($"Problem in getting person gender for {personId}");
                }
                return personGender;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                await _connection.CloseAsync();
            }
            return string.Empty;
        }

        private async Task<string> GetNameFromPersonId(string personId)
        {
            string? personName = null;
            try
            {
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"SELECT person_name " +
                    $"FROM mahakesher.person " +
                    $"WHERE person_id = '{personId}'", _connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        var value = reader.GetValue(0);
                        personName = value.ToString();
                    }
                }
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
            finally
            {
                await _connection.CloseAsync();
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
                string value = "";
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"SELECT {columnName} FROM {tableName} ORDER BY RAND ( )  LIMIT 1;", _connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        value = reader.GetValue(0).ToString();
                    }
                }
                return value;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                await _connection.CloseAsync();
            }
            return null;
        }

    }
}
