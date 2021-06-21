using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;

namespace Umbraco.Tools
{
    public class XmlDBRepository : IXmlRepository
    {
        public XmlDBRepository(IConfiguration config)
        {
            _conn = new SqlConnection(ConfigurationExtensions.GetConnectionString(config, "umbracoDbDSN"));
        }
        SqlConnection _conn;

        public IConfiguration Config { get; }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            _conn.Open();
            var lst = new List<XElement>();
            CreateTableIfNotExists();
            var sqlcmd = new SqlCommand("SELECT FROM MachineKeys", _conn);

            var reader = sqlcmd.ExecuteReader();
            while (reader.Read())
                lst.Add(XElement.Parse(reader["XML"].ToString()));
            reader.Close();
            _conn.Close();
            return new ReadOnlyCollection<XElement>(lst);

        }

        public void StoreElement(XElement element, string friendlyName)
        {
            _conn.Open();
            CreateTableIfNotExists();

            var sqlcmd = new SqlCommand("INSERT INTO MachineKeys (friendlyName, XML) VALUES (@friendlyName, @xml)", _conn);
            sqlcmd.Parameters.Add(new SqlParameter("friendlyName", friendlyName));
            sqlcmd.Parameters.Add(new SqlParameter("xml", element.ToString()));
            sqlcmd.ExecuteNonQuery();

            _conn.Close();
   
        }
        private void SafelyOpenConnection()
        {
            
        }
        private void CreateTableIfNotExists()
        {
            var cmd = "if not exists (select * from sysobjects where name='MachineKeys' and xtype='U') " +
                "create table MachineKeys( " +
                    "friendlyName varchar(250) not null, " +
                    "XML varchar(MAX) not null " +
                ")";
            var sqlcmd = new SqlCommand(cmd, _conn);
            sqlcmd.ExecuteNonQuery();

        }
    }
}
