/*
 * 작성자 : 남기연
 * 폼 설명 : 입퇴실; 진행 중인 예약에 대해 입실, 외출, 퇴실
*/

using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace UI
{
    public partial class Form7 : MetroFramework.Forms.MetroForm
    {
        DBClass dbc = new DBClass();

        private int memberId;

        public Form7(int memberId)
        {
            InitializeComponent();
            dbc.DB_ObjCreate();
            dbc.DB_Open();
            dbc.DB_Access();
            this.memberId = memberId;
            metroButton1.Visible = false;
            metroButton2.Visible = false;
            metroButton3.Visible = false;

            dbc.DS.Clear();
            string query = $"SELECT * FROM member WHERE member_no = {memberId}";
            dbc.DCom.CommandText = query;
            dbc.DA.SelectCommand = dbc.DCom;
            dbc.DA.Fill(dbc.DS, "member");
            dbc.MemberTable = dbc.DS.Tables["member"];
        }

        // 핀번호가 맞는지 확인
        private bool IsPasswordCorrect()
        {
            string enteredPassword = metroTextBox1.Text;
            string storedPassword = dbc.MemberTable.Rows[0]["password"].ToString();

            return enteredPassword == storedPassword;
        }

        // 핀번호가 일치하는 경우, 나머지 visible
        private void EnableEditing()
        {
            metroButton1.Visible = true;
            metroButton2.Visible = true;
            metroButton3.Visible = true;

            metroTextBox1.Enabled = false;
            metroLabel1.Enabled = false;
            metroButton4.Enabled = false;
        }

        // 핀 번호 확인 버튼 클릭 이벤트
        private void metroButton4_Click(object sender, EventArgs e)
        {
            if (IsPasswordCorrect())
            {
                EnableEditing();
            }
            else
            {
                MessageBox.Show("핀이 일치하지 않습니다.");
            }
        }

        // 입퇴실 폼 닫기 버튼 클릭 이벤트
        private void metroButton5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 입실 버튼 클릭 이벤트
        private void metroButton1_Click(object sender, EventArgs e)
        {
            try
            {
                dbc.DS.Clear();

                string query = "SELECT * FROM reservation";
                dbc.DCom.CommandText = query;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservation");
                dbc.ReservationTable = dbc.DS.Tables["reservation"];

                string cioQuery = "SELECT * FROM member_check_in_out";
                dbc.DCom.CommandText = cioQuery;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "member_check_in_out");
                dbc.CheckioTable = dbc.DS.Tables["member_check_in_out"];

                DataRow[] activeReservationRows = dbc.ReservationTable.Select($"rsrv_result = '진행 중' AND member_no = {memberId}");
                if (activeReservationRows.Length > 0)
                {
                    int reservationNo = Convert.ToInt32(activeReservationRows[0]["rsrv_no"]);

                    DataRow[] existingCheckInOutRows = dbc.CheckioTable.Select($"rsrv_no = {reservationNo} AND member_no = {memberId} AND check_in_time IS NOT NULL");

                    if (existingCheckInOutRows.Length == 0)
                    {
                        string insertCheckInOutQuery = $"INSERT INTO member_check_in_out (rsrv_no, member_no, check_in_time, check_out_time) " +
                               $"VALUES ({reservationNo}, {memberId}, SYSTIMESTAMP, NULL)";

                        dbc.DCom.CommandText = insertCheckInOutQuery;
                        dbc.DCom.ExecuteNonQuery();

                        MessageBox.Show("입실 성공!");

                        Form3 form3 = Application.OpenForms.OfType<Form3>().FirstOrDefault();
                        if (form3 != null)
                        {
                            form3.Close();
                        }

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("이미 입실하셨습니다.");
                    }
                }
                else
                {
                    MessageBox.Show("진행 중인 예약이 없습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"입실 중 오류 발생: {ex.Message}");
            }
        }

        // 외출 버튼 클릭 이벤트
        private void metroButton2_Click(object sender, EventArgs e)
        {
            try
            {
                dbc.DS.Clear();

                string query = "SELECT * FROM reservation";
                dbc.DCom.CommandText = query;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservation");
                dbc.ReservationTable = dbc.DS.Tables["reservation"];

                string cioQuery = "SELECT * FROM member_check_in_out";
                dbc.DCom.CommandText = cioQuery;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "member_check_in_out");
                dbc.CheckioTable = dbc.DS.Tables["member_check_in_out"];

                DataRow[] activeReservationRows = dbc.ReservationTable.Select($"rsrv_result = '진행 중' AND member_no = {memberId}");
                if (activeReservationRows.Length > 0)
                {
                    int reservationNo = Convert.ToInt32(activeReservationRows[0]["rsrv_no"]);

                    DataRow[] existingCheckInOutRows = dbc.CheckioTable.Select($"rsrv_no = {reservationNo} AND member_no = {memberId} AND check_in_time IS NOT NULL");

                    if (existingCheckInOutRows.Length > 0)
                    {
                        Form3 form3 = Application.OpenForms.OfType<Form3>().FirstOrDefault();
                        if (form3 != null)
                        {
                            form3.Close();
                        }

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("입실을 하지 않으셨습니다. 먼저 입실해주세요.");
                    }
                }
                else
                {
                    MessageBox.Show("진행 중인 예약이 없습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"외출 중 오류 발생: {ex.Message}");
            }
        }

        // 퇴실 버튼 클릭 이벤트
        private void metroButton3_Click(object sender, EventArgs e)
        {
            try
            {
                dbc.DS.Clear();

                string query = "SELECT * FROM reservation";
                dbc.DCom.CommandText = query;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservation");
                dbc.ReservationTable = dbc.DS.Tables["reservation"];

                string cioQuery = "SELECT * FROM member_check_in_out";
                dbc.DCom.CommandText = cioQuery;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "member_check_in_out");
                dbc.CheckioTable = dbc.DS.Tables["member_check_in_out"];

                DataRow[] activeReservationRows = dbc.ReservationTable.Select($"rsrv_result = '진행 중' AND member_no = {memberId}");

                if (activeReservationRows.Length > 0)
                {
                    int reservationNo = Convert.ToInt32(activeReservationRows[0]["rsrv_no"]);

                    DataRow[] existingCheckOutRows = dbc.CheckioTable.Select($"rsrv_no = {reservationNo} AND member_no = {memberId} AND check_in_time IS NOT NULL AND check_out_time IS NULL");

                    if (existingCheckOutRows.Length > 0)
                    {
                        DialogResult result = MessageBox.Show("퇴실하시겠습니까?", "퇴실 확인", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            DateTime checkInTime = Convert.ToDateTime(existingCheckOutRows[0]["check_in_time"]);

                            string updateCheckOutQuery = $"UPDATE member_check_in_out SET check_out_time = SYSTIMESTAMP " +
                                $"WHERE rsrv_no = {reservationNo}";

                            dbc.DCom.CommandText = updateCheckOutQuery;
                            dbc.DCom.ExecuteNonQuery();

                            MessageBox.Show("퇴실 성공!");

                            Form3 form3 = Application.OpenForms.OfType<Form3>().FirstOrDefault();
                            if (form3 != null)
                            {
                                form3.Close();
                            }
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("입실을 하지 않으셨습니다. 먼저 입실해주세요.");
                    }
                }
                else
                {
                    MessageBox.Show("진행 중인 예약이 없습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"퇴실 중 오류 발생: {ex.Message}");
            }
        }
    }
}
