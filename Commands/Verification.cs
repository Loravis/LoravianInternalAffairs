using Discord;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System.Net;
using CrypticWizard.RandomWordGenerator;
using static CrypticWizard.RandomWordGenerator.WordGenerator;

namespace LoravianInternalAffairs.Commands
{
    public static class Verification
    {
        public static async Task<bool> CheckIfVerified(string robloxId, string discordUserId, string mysqlPassword)
        {
            string cs = @"server=localhost;userid=Loraviis;password=" + mysqlPassword + ";database=playerdata";
            using var con = new MySqlConnection(cs);
            con.Open();

            var cmd = new MySqlCommand("SELECT *\r\nFROM verifications\r\nWHERE robloxid LIKE '%" + robloxId + "%';\r\n", con);
            var result = await cmd.ExecuteScalarAsync();
            if (result == null)
            {
                return false;  
            } else
            {
                return true;
            }
        }

        public static string InitiateVerification()
        {
            WordGenerator myWordGenerator = new WordGenerator();
            string word = myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " " + myWordGenerator.GetWord(PartOfSpeech.noun) + " ";
            return word;
        }
    }
}
