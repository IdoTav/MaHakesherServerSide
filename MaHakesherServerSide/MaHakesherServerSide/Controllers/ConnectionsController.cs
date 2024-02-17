using Humanizer;
using MaHakesherServerSide.Data;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace MaHakesherServerSide.Controllers
{
    public class ConnectionsController : Controller
    {
        private readonly MySqlConnection _connection;

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

        public async Task<Dictionary<string, string>?> GetRelations (string name){
            Dictionary<string, string> relationsDictionary = new Dictionary<string, string>();
            try
            {
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"USE mahakesher; " +
                    $"SELECT person_id_2 AS other_person, relationship_type " +
                    $"FROM mahakesher.relationship " +
                    $"WHERE person_id_1 ='{name}';", _connection);
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
                                                     $"  GROUP BY pv.person_id) "+
                                                     $", ReferenceRange AS( "+
                                                     $"  SELECT reference_id" +
                                                     $"  FROM mahakesher.bibledata_reference " +
                                                     $"  WHERE verse_sequence BETWEEN (SELECT start_value FROM NameReference) AND (SELECT end_value FROM NameReference) " +
                                                     $"  )"+
                                                     $"SELECT DISTINCT pv.person_id "+
                                                     $"FROM mahakesher.person_verse pv "+
                                                     $"INNER JOIN ReferenceRange rr ON pv.reference_id = rr.reference_id "+
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

        public async Task<string?> RandomRawAsync(string tableName, string columnName)
        {
            try
            {
                await _connection.OpenAsync();
                using var command = new MySqlCommand($"SELECT {columnName} FROM {tableName}  ORDER BY RAND ( )  LIMIT 1;", _connection);
                using var reader = await command.ExecuteReaderAsync();
                await _connection.CloseAsync();
                while (reader.Read())
                {
                    string value = reader.GetValue(0).ToString();
                    return value;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return null;
        }

    }
}
