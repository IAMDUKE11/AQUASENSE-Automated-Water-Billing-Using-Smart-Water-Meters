using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using WaterMeterAPI.Models;

namespace WaterMeterAPI.Data
{
    public class FirebirdHelper
    {
        private readonly string _connectionString;

        public FirebirdHelper(string databasePath)
        {
            _connectionString = $"User=SYSDBA;Password=admin;Database={databasePath};DataSource=localhost;Port=3050;Charset=UTF8;Pooling=True;";
        }

        #region WaterReadings
        public List<WaterReading> GetAllWaterReadings()
        {
            var list = new List<WaterReading>();
            using var con = new FbConnection(_connectionString);
            con.Open();
            using var cmd = new FbCommand(
                "SELECT Id, Meter_Id, Flow_Rate, Total_Liters, Bill, Reading_Time FROM water_readings", con);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new WaterReading
                {
                    Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                    MeterId = reader["Meter_Id"]?.ToString() ?? "",
                    FlowRate = reader["Flow_Rate"] != DBNull.Value ? Convert.ToDouble(reader["Flow_Rate"]) : 0,
                    TotalLiters = reader["Total_Liters"] != DBNull.Value ? Convert.ToDouble(reader["Total_Liters"]) : 0,
                    Bill = reader["Bill"] != DBNull.Value ? Convert.ToDouble(reader["Bill"]) : 0,
                    ReadingTime = reader["Reading_Time"] != DBNull.Value ? (DateTime)reader["Reading_Time"] : DateTime.MinValue
                });
            }

            return list;
        }

        public WaterReading? GetLatestWaterReading()
        {
            using var con = new FbConnection(_connectionString);
            con.Open();
            using var cmd = new FbCommand(
                @"SELECT FIRST 1 Id, Meter_Id, Flow_Rate, Total_Liters, Bill, Reading_Time
          FROM water_readings
          ORDER BY Reading_Time DESC", con);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return new WaterReading
            {
                Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                MeterId = reader["Meter_Id"]?.ToString() ?? "",
                FlowRate = reader["Flow_Rate"] != DBNull.Value ? Convert.ToDouble(reader["Flow_Rate"]) : 0,
                TotalLiters = reader["Total_Liters"] != DBNull.Value ? Convert.ToDouble(reader["Total_Liters"]) : 0,
                Bill = reader["Bill"] != DBNull.Value ? Convert.ToDouble(reader["Bill"]) : 0,
                ReadingTime = reader["Reading_Time"] != DBNull.Value ? (DateTime)reader["Reading_Time"] : DateTime.MinValue
            };
        }
        public void AddWaterReading(WaterReading reading)
        {
            using var con = new FbConnection(_connectionString);
            con.Open();
            using var cmd = new FbCommand(
                @"INSERT INTO water_readings (Meter_Id, Flow_Rate, Total_Liters, Bill, Reading_Time)
                  VALUES (@meterId, @flow, @total, @bill, @time)", con);

            cmd.Parameters.Add("@meterId", FbDbType.VarChar).Value = reading.MeterId ?? "";
            cmd.Parameters.Add("@flow", FbDbType.Float).Value = reading.FlowRate;
            cmd.Parameters.Add("@total", FbDbType.Float).Value = reading.TotalLiters;
            cmd.Parameters.Add("@bill", FbDbType.Float).Value = reading.Bill;
            cmd.Parameters.Add("@time", FbDbType.TimeStamp).Value = reading.ReadingTime != default ? reading.ReadingTime : DateTime.Now;

            cmd.ExecuteNonQuery();
        }

        #endregion

        #region Users

        public List<User> GetAllUsers()
        {
            var list = new List<User>();
            using var con = new FbConnection(_connectionString);
            con.Open();
            using var cmd = new FbCommand(
                "SELECT Id, Name, Address, Meter_Id FROM users", con);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new User
                {
                    Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                    Name = reader["Name"] != DBNull.Value ? reader["Name"]!.ToString()! : "",
                    Address = reader["Address"] != DBNull.Value ? reader["Address"]!.ToString()! : "",
                    MeterId = reader["Meter_Id"] != DBNull.Value ? reader["Meter_Id"]!.ToString()! : ""
                });
            }

            return list;
        }

        public void AddUser(User user)
        {
            using var con = new FbConnection(_connectionString);
            con.Open();
            using var cmd = new FbCommand(
                @"INSERT INTO users (Name, Address, Meter_Id) VALUES (@name, @address, @meterId)", con);

            cmd.Parameters.Add("@name", FbDbType.VarChar).Value = user.Name ?? "";
            cmd.Parameters.Add("@address", FbDbType.VarChar).Value = user.Address ?? "";
            cmd.Parameters.Add("@meterId", FbDbType.VarChar).Value = user.MeterId ?? "";

            cmd.ExecuteNonQuery();
        }

        #endregion

        #region Alerts

        public List<Alert> GetAllAlerts()
        {
            var list = new List<Alert>();
            using var con = new FbConnection(_connectionString);
            con.Open();
            using var cmd = new FbCommand(
                "SELECT Id, Meter_Id, Alert_Type, Message, Created_At FROM alerts", con);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Alert
                {
                    Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                    MeterId = reader["Meter_Id"] != DBNull.Value ? reader["Meter_Id"]!.ToString()! : "",
                    AlertType = reader["Alert_Type"] != DBNull.Value ? reader["Alert_Type"]!.ToString()! : "",
                    Message = reader["Message"] != DBNull.Value ? reader["Message"]!.ToString()! : "",
                    CreatedAt = reader["Created_At"] != DBNull.Value ? (DateTime)reader["Created_At"]! : DateTime.MinValue
                });
            }

            return list;
        }

        public void AddAlert(Alert alert)
        {
            using var con = new FbConnection    (_connectionString);
            con.Open();
            using var cmd = new FbCommand(
                @"INSERT INTO alerts (Meter_Id, Alert_Type, Message, Created_At)
                  VALUES (@meterId, @type, @message, @createdAt)", con);

            cmd.Parameters.Add("@meterId", FbDbType.VarChar).Value = alert.MeterId ?? "";
            cmd.Parameters.Add("@type", FbDbType.VarChar).Value = alert.AlertType ?? "";
            cmd.Parameters.Add("@message", FbDbType.VarChar).Value = alert.Message ?? "";
            cmd.Parameters.Add("@createdAt", FbDbType.TimeStamp).Value = alert.CreatedAt != default ? alert.CreatedAt : DateTime.Now;

            cmd.ExecuteNonQuery();
        }

        #endregion
    }
}