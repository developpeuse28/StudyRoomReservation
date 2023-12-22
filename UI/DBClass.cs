using System;
using System.Data;
using System.Windows.Forms;
using Oracle.DataAccess.Client;

namespace UI
{
    public class DBClass
    {
        OracleConnection con;
        OracleCommand dcom;
        OracleDataAdapter da;

        DataSet ds;
        OracleDataReader dr;
        OracleCommandBuilder myCommandBuilder;
        DataTable lockerTable, memberTable, warningTable, seatTable, reservationTypeTable, reservationTable, checkioTable;

        public OracleConnection Con { get { return con; } set { con = value; } }
        public OracleCommand DCom { get { return dcom; } set { dcom = value; } }
        public OracleDataAdapter DA { get { return da; } set { da = value; } }
        public OracleDataReader DR { get { return dr; } set { dr = value; } }
        public DataSet DS { get { return ds; } set { ds = value; } }
        public OracleCommandBuilder MyCommandBuilder { get { return myCommandBuilder; } set { myCommandBuilder = value; } }
        public DataTable LockerTable { get { return lockerTable; } set { lockerTable = value; } }
        public DataTable MemberTable { get { return memberTable; } set { memberTable = value; } }
        public DataTable WarningTable { get { return warningTable; } set { warningTable = value; } }
        public DataTable SeatTable { get { return seatTable; } set { seatTable = value; } }
        public DataTable ReservationTypeTable { get { return reservationTypeTable; } set { reservationTypeTable = value; } }
        public DataTable ReservationTable { get { return reservationTable; } set { reservationTable = value; } }
        public DataTable CheckioTable { get { return checkioTable; } set { checkioTable = value; } }

        public void DB_Open()
        {
            try
            {
                string connectionString = "User Id=dock; Password=1111; Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521)) (CONNECT_DATA = (SERVER = DEDICATED) (SERVICE_NAME = xe) ));";

                foreach (DataTable table in GetAllTables())
                {
                    string commandString = $"SELECT * FROM {table.TableName}";

                    DA = new OracleDataAdapter(commandString, connectionString);
                    MyCommandBuilder = new OracleCommandBuilder(DA);

                    DS = new DataSet();
                }
            }
            catch (DataException DE)
            {
                MessageBox.Show(DE.Message);
            }
        }
        private DataTable[] GetAllTables()
        {
            return new DataTable[] { LockerTable, MemberTable, WarningTable, SeatTable, ReservationTypeTable, ReservationTable, CheckioTable };
        }

        public decimal GetNextSequenceValue(string sequenceName)
        {
            try
            {
                string connectionString = "User Id=dock; Password=1111; Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521)) (CONNECT_DATA = (SERVER = DEDICATED) (SERVICE_NAME = xe) ));";

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();

                    using (OracleCommand command = new OracleCommand($"SELECT {sequenceName}.NEXTVAL FROM DUAL", connection))
                    {
                        return (decimal)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }

        public void DB_Access()
        {
            try
            {
                string My_con = "User Id=dock; Password=1111; Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521)) (CONNECT_DATA = (SERVER = DEDICATED) (SERVICE_NAME = xe) ));";

                Con = new OracleConnection();
                Con.ConnectionString = My_con;
                DCom = new OracleCommand();
                DCom.Connection = Con;
                Con.Open();
            }
            catch (DataException DE)
            {
                MessageBox.Show(DE.Message);
            }
        }

        public void DB_ObjCreate()
        {
            LockerTable = new DataTable();
            MemberTable = new DataTable();
            WarningTable = new DataTable();
            SeatTable = new DataTable();
            ReservationTypeTable = new DataTable();
            ReservationTable = new DataTable();
            CheckioTable = new DataTable();
        }
    }
}
