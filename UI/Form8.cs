/*
 * 작성자 : 남기연
 * 폼 설명 : 예약 수정 화면; Form6 예약 관리 화면의 dataGridView에서 진행 중인 예약 행에 오른쪽 마우스 클릭 후 '선택한 행 수정'으로 들어온 화면
*/

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Controls;

namespace UI
{
    public partial class Form8 : MetroFramework.Forms.MetroForm
    {
        DBClass dbc = new DBClass();

        private int memberId;
        private string membershipType;
        private string selectedPreviousSeat;
        private string selectedSeat;
        private DataRow selectedDataRow;
        private MetroButton lastClickedButton;

        public Form8(int memberId)
        {
            InitializeComponent();
            dbc.DB_ObjCreate();
            dbc.DB_Open();
            dbc.DB_Access();
            this.memberId = memberId;
        }

        // 수정하기 위해 선택된 행의 데이터
        public void SetDataForUpdate(DataRow dataRow)
        {
            selectedDataRow = dataRow;
        }

        private void Form8_Load(object sender, EventArgs e)
        {
            PopulateListView(membershipType);
            metroTextBox5.Text = selectedDataRow["rsrv_no"].ToString();
            metroTextBox5.Enabled = false;
            metroTextBox2.Text = selectedDataRow["rsrv_sc"].ToString();
            metroTextBox3.Text = selectedDataRow["rsrv_tc"].ToString();
            selectedPreviousSeat = selectedDataRow["rsrv_sc"].ToString();
        }

        // 멤버십 유형에 따라 예약 종류 리스트뷰에 뿌려줄 데이터를 달리 함
        private void PopulateListView(string membershipType)
        {
            listView1.Items.Clear();

            string query = $"SELECT rsrv_tc, rsrv_name, rsrv_time, price FROM reservation_type";
            dbc.DCom.CommandText = query;
            dbc.DA.SelectCommand = dbc.DCom;
            dbc.DA.Fill(dbc.DS, "reservation_type");
            dbc.ReservationTypeTable = dbc.DS.Tables["reservation_type"];

            foreach (DataRow row in dbc.DS.Tables["reservation_type"].Rows)
            {
                string rsrv_tc = row["rsrv_tc"].ToString();

                if (membershipType == "월회원")
                {
                    metroTextBox3.Text = "M";
                }

                if ((membershipType == "월회원" && rsrv_tc == "M") ||
                    (membershipType == "일반회원" && (rsrv_tc == "H1" || rsrv_tc == "H2" || rsrv_tc == "H4" || rsrv_tc == "H8" || rsrv_tc == "D")))
                {
                    ListViewItem item = new ListViewItem(rsrv_tc);
                    item.SubItems.Add(row["rsrv_name"].ToString());
                    item.SubItems.Add(row["rsrv_time"].ToString());
                    item.SubItems.Add(row["price"].ToString());
                    listView1.Items.Add(item);
                }
            }
        }

        // 좌석 버튼 enabled false로 초기화
        private void InitializeSeatButtons()
        {
            foreach (Control control in groupBox3.Controls)
            {
                if (control is MetroButton seatButton)
                {
                    seatButton.Enabled = false;
                    seatButton.Click += SeatButton_Click;
                }
            }
        }

        // 좌석 버튼 클릭 이벤트
        private void SeatButton_Click(object sender, EventArgs e)
        {
            if (lastClickedButton != null)
            {
                lastClickedButton.BackColor = SystemColors.Control;
                lastClickedButton.Highlight = false;
            }

            MetroButton clickedButton = (MetroButton)sender;
            clickedButton.BackColor = Color.LightBlue;
            clickedButton.Highlight = true;
            selectedSeat = clickedButton.Name;

            lastClickedButton = clickedButton;
        }

        // 사용이 가능한 좌석 검색 버튼 클릭 이벤트
        private void metroButton44_Click(object sender, EventArgs e)
        {
            try
            {
                dbc.DS.Clear();

                string query = $"SELECT * FROM member WHERE member_no = {memberId}";
                dbc.DCom.CommandText = query;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "member");
                dbc.MemberTable = dbc.DS.Tables["member"];

                string sQuery = $"SELECT * FROM seat";
                dbc.DCom.CommandText = sQuery;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "seat");
                dbc.SeatTable = dbc.DS.Tables["seat"];

                InitializeSeatButtons();
                string gender = dbc.MemberTable.Rows[0]["gender"].ToString();

                foreach (Control control in groupBox3.Controls)
                {
                    if (control is MetroButton seatButton)
                    {
                        string seatName = seatButton.Name;

                        if ((gender == "여" && seatName.StartsWith("A")) || (gender == "남" && seatName.StartsWith("B")))
                        {
                            DataRow[] seatRows = dbc.SeatTable.Select($"seat_code = '{seatName}'");

                            if (seatRows.Length > 0)
                            {
                                string seatStatus = seatRows[0]["seat_state"].ToString();

                                if (seatStatus == "사용 중" && seatName != selectedPreviousSeat)
                                {
                                    seatButton.Enabled = false;
                                }
                                else
                                {
                                    seatButton.Enabled = true;
                                }
                            }
                            else
                            {
                                seatButton.Enabled = false;
                            }
                        }
                        else
                        {
                            seatButton.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in metroButton44_Click: {ex.Message}");
            }

        }

        // 좌석 선택 버튼 클릭 이벤트 - 좌석코드 textbox에 값이 채워짐
        private void metroButton3_Click(object sender, EventArgs e)
        {
            bool seatSelected = false;
            foreach (Control control in groupBox3.Controls)
            {
                if (control is MetroButton seatButton && seatButton.BackColor == Color.LightBlue)
                {
                    seatSelected = true;
                    metroTextBox2.Text = seatButton.Name;
                }
            }
            if (!seatSelected)
            {
                MessageBox.Show("좌석을 선택하세요.");
            }
        }

        // 예약 종류 리스트뷰에서 항목을 선택하면(해당 줄 아무 곳이나), 예약의 예약종류코드 textbox에 값이 채워짐
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = listView1.HitTest(e.Location);

            if (hit.Item != null)
            {
                hit.Item.Selected = true;

                metroTextBox3.Text = hit.Item.SubItems[0].Text;
            }
        }

        // Form6으로부터 멤버십 유형을 받아 저장 - 예약 종류 리스트에 뿌려줄 데이터가 달라짐
        public void SetMemberTypeForUpdate(string memberType)
        {
            membershipType = memberType;
        }

        // 예약 정보 수정 버튼 클릭 이벤트
        private void metroButton1_Click(object sender, EventArgs e)
        {
            UpdateReservationData();

            this.Close();
        }

        // 예약 테이블의 데이터 수정
        private void UpdateReservationData()
        {
            try
            {
                int reservationNumber = Convert.ToInt32(selectedDataRow["rsrv_no"]);
                selectedSeat = metroTextBox2.Text.ToString();
                string selectedType = metroTextBox3.Text.ToString();

                string updateReservationQuery = $"UPDATE reservation SET rsrv_sc = '{selectedSeat}', rsrv_tc = '{selectedType}' WHERE rsrv_no = {reservationNumber}";

                dbc.DCom.CommandText = updateReservationQuery;
                dbc.DCom.ExecuteNonQuery();

                string updateSeatQuery = $"UPDATE seat SET seat_state = '사용 가능' WHERE seat_code = '{selectedPreviousSeat}'";

                dbc.DCom.CommandText = updateSeatQuery;
                dbc.DCom.ExecuteNonQuery();

                string updateSeat2Query = $"UPDATE seat SET seat_state = '사용 중' WHERE seat_code = '{selectedSeat}'";

                dbc.DCom.CommandText = updateSeat2Query;
                dbc.DCom.ExecuteNonQuery();

                MessageBox.Show("예약 수정 성공!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating reservation data: {ex.Message}");
            }
        }

        // 예약 수정 폼 닫기 버튼 클릭 이벤트
        private void metroButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
