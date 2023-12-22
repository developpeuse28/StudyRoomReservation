/*
 * 작성자 : 남기연
 * 폼 설명 : 예약 관리; 로그인한 회원의 진행 중인 예약 정보 수정, 삭제 가능
*/

using System;
using System.Data;
using System.Windows.Forms;

namespace UI
{
    public partial class Form6 : MetroFramework.Forms.MetroForm
    {
        DBClass dbc = new DBClass();

        private int memberId;
        private string membershipType;
        private DataTable reservationTable;

        public Form6(int memberId)
        {
            InitializeComponent();
            dbc.DB_ObjCreate();
            dbc.DB_Open();
            dbc.DB_Access();
            this.memberId = memberId;
            groupBox1.Visible = false;
            groupBox2.Visible = false;
            metroLabel4.Visible = false;
            dataGridView1.Visible = false;

            dbc.DS.Clear();

            string query = $"SELECT * FROM member WHERE member_no = {memberId}";
            dbc.DCom.CommandText = query;
            dbc.DA.SelectCommand = dbc.DCom;
            dbc.DA.Fill(dbc.DS, "member");
            dbc.MemberTable = dbc.DS.Tables["member"];

            if (dbc.MemberTable.Rows.Count > 0)
            {
                membershipType = dbc.MemberTable.Rows[0]["membership_type"].ToString();
            }

            showDataGridView();
        }

        // 데이터그리드뷰에 예약 데이터를 뿌림
        private void showDataGridView()
        {
            try
            {
                string query = "SELECT * FROM reservation";
                dbc.DCom.CommandText = query;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservation");
                dbc.ReservationTable = dbc.DS.Tables["reservation"];

                string rtQuery = "SELECT * FROM reservation_type";
                dbc.DCom.CommandText = rtQuery;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservation_type");
                dbc.ReservationTypeTable = dbc.DS.Tables["reservation_type"];

                string cioQuery = "SELECT * FROM member_check_in_out";
                dbc.DCom.CommandText = cioQuery;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "member_check_in_out");
                dbc.CheckioTable = dbc.DS.Tables["member_check_in_out"];

                string reservationQuery = $@"SELECT r.rsrv_no, r.rsrv_sc, r.rsrv_tc, r.rsrv_st, 
                                    CASE WHEN mco.check_out_time < r.rsrv_st + (rt.rsrv_time * INTERVAL '1' HOUR) 
                                        THEN mco.check_out_time 
                                        ELSE r.rsrv_st + (rt.rsrv_time * INTERVAL '1' HOUR) 
                                    END AS rsrv_et, r.payment_method, r.rsrv_result 
                                    FROM reservation r 
                                    JOIN reservation_type rt ON r.rsrv_tc = rt.rsrv_tc 
                                    LEFT JOIN member_check_in_out mco ON r.rsrv_no = mco.rsrv_no 
                                    WHERE r.member_no = {memberId}";

                dbc.DA.SelectCommand.CommandText = reservationQuery;
                dbc.DS.Tables.Remove("reservation");
                dbc.DCom.CommandText = reservationQuery;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservation");
                dbc.ReservationTable = dbc.DS.Tables["reservation"];

                reservationTable = dbc.ReservationTable.Copy();
                dataGridView1.DataSource = dbc.DS.Tables["reservation"];
                dataGridView1.Refresh();
                dataGridView1.Columns.Clear();
                dataGridView1.AutoGenerateColumns = false;

                dataGridView1.Columns.Add("RSRV_NO", "예약번호");
                dataGridView1.Columns["RSRV_NO"].DataPropertyName = "RSRV_NO";
                dataGridView1.Columns.Add("RSRV_SC", "좌석코드");
                dataGridView1.Columns["RSRV_SC"].DataPropertyName = "RSRV_SC";
                dataGridView1.Columns.Add("RSRV_TC", "예약종류코드");
                dataGridView1.Columns["RSRV_TC"].DataPropertyName = "RSRV_TC";
                dataGridView1.Columns.Add("RSRV_ST", "시작시간");
                dataGridView1.Columns["RSRV_ST"].DataPropertyName = "RSRV_ST";
                dataGridView1.Columns.Add("RSRV_ET", "마감시간");
                dataGridView1.Columns["RSRV_ET"].DataPropertyName = "RSRV_ET";
                dataGridView1.Columns.Add("PAYMENT_METHOD", "결제수단");
                dataGridView1.Columns["PAYMENT_METHOD"].DataPropertyName = "PAYMENT_METHOD";
                dataGridView1.Columns.Add("RSRV_RESULT", "처리결과");
                dataGridView1.Columns["RSRV_RESULT"].DataPropertyName = "RSRV_RESULT";

                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                metroLabel4.Text = $"총 {dbc.DS.Tables["reservation"].Rows.Count}회";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터그리드뷰 출력 중 오류 발생: {ex.Message}");
            }
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
            groupBox1.Visible = true;
            groupBox2.Visible = true;
            metroLabel4.Visible = true;
            dataGridView1.Visible = true;

            metroTextBox1.Enabled = false;
            metroLabel1.Enabled = false;
            metroButton4.Enabled = false;
        }

        // 예약 관리 폼 닫기 버튼 클릭 이벤트
        private void metroButton3_Click(object sender, EventArgs e)
        {
            this.Close();
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

        // 예약번호 검색 확인 버튼 클릭 이벤트
        private void metroButton2_Click(object sender, EventArgs e)
        {
            string searchValue = metroTextBox6.Text;
            DataRow[] foundRows = dbc.DS.Tables["reservation"].Select($"rsrv_no = '{searchValue}'");

            if (foundRows.Length > 0)
            {
                DataTable searchResultTable = foundRows.CopyToDataTable();
                dataGridView1.DataSource = searchResultTable;

                metroLabel4.Text = $"총 {searchResultTable.Rows.Count}회";
            }
            else
            {
                MessageBox.Show($"{searchValue}번으로 예약된 내역이 없습니다.");
                metroTextBox6.Clear();
            }
        }

        // 예약번호 검색 이전 버튼 클릭 이벤트
        private void metroButton8_Click(object sender, EventArgs e)
        {
            metroTextBox6.Clear();
            dataGridView1.DataSource = reservationTable;
            metroLabel4.Text = $"총 {reservationTable.Rows.Count}회";
        }

        // 데이터그리드뷰의 데이터를 정렬
        private void SortDataGridView(string columnName)
        {
            try
            {
                DataView dv = reservationTable.DefaultView;

                if (columnName == "rsrv_result")
                {
                    dv.Sort = $"{columnName} DESC";
                }
                else
                {
                    dv.Sort = columnName;
                }
                dataGridView1.DataSource = dv.ToTable();

                metroLabel4.Text = $"총 {dv.Count}회";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터그리드뷰 정렬 중 오류 발생: {ex.Message}");
            }
        }

        // 예약번호에 따라 정렬
        private void metroButton6_Click(object sender, EventArgs e)
        {
            SortDataGridView("rsrv_no");
        }

        // 예약종류에 따라 정렬
        private void metroButton5_Click(object sender, EventArgs e)
        {
            SortDataGridView("rsrv_tc");
        }

        // 예약 처리결과에 따라 정렬
        private void metroButton1_Click(object sender, EventArgs e)
        {
            SortDataGridView("rsrv_result");
        }

        // 데이터그리드뷰에서 선택한 행 수정 메뉴 클릭 이벤트
        private void 선택한행수정ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                DataRowView dataRow = (DataRowView)selectedRow.DataBoundItem;

                if (dataRow.Row["rsrv_result"].ToString() == "진행 중")
                {
                    Form8 form8 = new Form8(memberId);
                    form8.SetDataForUpdate(dataRow.Row);
                    form8.SetMemberTypeForUpdate(membershipType);
                    form8.ShowDialog();
                    showDataGridView();
                }
                else
                {
                    MessageBox.Show("선택된 예약은 이미 완료 혹은 취소되었기 때문에 수정할 수 없습니다.");
                }
            }

            else
            {
                MessageBox.Show("선택된 행이 없습니다.");
            }
        }

        // 데이터그리드뷰에서 선택한 행 삭제 메뉴 클릭 이벤트
        private void 선택한행삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                DataRowView dataRow = (DataRowView)selectedRow.DataBoundItem;

                if (dataRow.Row["rsrv_result"].ToString() == "진행 중")
                {
                    DialogResult result = MessageBox.Show("예약을 취소하시겠습니까?", "취소 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            int reservationNumber = Convert.ToInt32(dataRow.Row["rsrv_no"]);
                            string selectedSeat = dataRow.Row["rsrv_sc"].ToString();

                            string updateReservationQuery = $"DELETE FROM reservation WHERE rsrv_no = {reservationNumber}";

                            dbc.DCom.CommandText = updateReservationQuery;
                            dbc.DCom.ExecuteNonQuery();

                            string updateSeatQuery = $"UPDATE seat SET seat_state = '사용 가능', seat_code = '{selectedSeat}' WHERE seat_code = '{selectedSeat}'";

                            dbc.DCom.CommandText = updateSeatQuery;
                            dbc.DCom.ExecuteNonQuery();

                            MessageBox.Show("예약 취소 성공!");
                            showDataGridView();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"예약 취소 중 오류 발생: {ex.Message}");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("선택된 예약은 이미 완료 혹은 취소되었기 때문에 취소할 수 없습니다.");
                }
            }
            else
            {
                MessageBox.Show("선택된 행이 없습니다.");
            }
        }
    }
}
