/*
 * 작성자 : 남기연
 * 폼 설명 : 메인 화면; 로그인한 회원에 해당하는 회원 정보, 경고 내역 보기만 가능
 *           좌석 검색, 선택, 사물함 검색, 선택, 예약 추가, 예약 내역 보기 가능. 즉 데이터 검색, 삽입이 주로 이루어지고, 수정, 삭제는 없음
*/

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Controls;

namespace UI
{
    public partial class Form4 : MetroFramework.Forms.MetroForm
    {
        private int memberId;
        private int selectedMemberId;
        private string selectedSeat;
        private string selectedLocker;
        private Form3 mainForm;
        private string memberState;
        private MetroButton lastClickedButton;

        DBClass dbc = new DBClass();

        public Form4(int memberId, Form3 mainForm)
        {
            InitializeComponent();
            dbc.DB_ObjCreate();
            dbc.DB_Open();
            dbc.DB_Access();
            this.memberId = memberId;
            this.mainForm = mainForm;
            UpdateReservationStatusAndInsertWarning();
            UpdateMemberInformation();
            LoadMemberData();
        }

        // 좌석 상태 사용 가능으로 업데이트 (예약 마감 시간이 지나 자동 완료되어 좌석 반환)
        private void UpdateSeatState(string seatCode)
        {
            try
            {
                string updateSeatStateQuery = $"UPDATE seat SET seat_state = '사용 가능' WHERE seat_code = '{seatCode}'";

                dbc.DCom.CommandText = updateSeatStateQuery;
                dbc.DCom.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating seat state: {ex.Message}");
            }
        }

        // 예약 마감 시간까지 퇴실하지 않은 모든 회원들의 예약에 대해, 예약 상태 완료로 업데이트하고 해당 회원에게 경고 부여
        private void UpdateReservationStatusAndInsertWarning()
        {
            try
            {
                string selectReservationsQuery = $"SELECT r.rsrv_no, r.rsrv_st, r.rsrv_tc, r.rsrv_sc, m.check_out_time, r.member_no " +
                           $"FROM reservation r " +
                           $"LEFT JOIN member_check_in_out m ON r.rsrv_no = m.rsrv_no " +
                           $"WHERE r.rsrv_result = '진행 중'";

                dbc.DCom.CommandText = selectReservationsQuery;
                dbc.DR = dbc.DCom.ExecuteReader();
                
                while (dbc.DR.Read())
                {
                    selectedMemberId = Convert.ToInt32(dbc.DR["member_no"]);
                    int reservationNo = Convert.ToInt32(dbc.DR["rsrv_no"]);
                    DateTime reservationStartTime = Convert.ToDateTime(dbc.DR["rsrv_st"]);
                    string reservationTypeCode = dbc.DR["rsrv_tc"].ToString();
                    DateTime? checkOutTime = dbc.DR["check_out_time"] as DateTime?;

                    int reservationTypeDuration = GetReservationTypeDuration(reservationTypeCode);

                    //DateTime reservationClosingTime = reservationStartTime.AddSeconds(reservationTypeDuration);
                    DateTime reservationClosingTime = reservationStartTime.AddHours(reservationTypeDuration);

                    if (DateTime.Now > reservationClosingTime || checkOutTime != null)
                    {
                        string updateReservationStatusQuery = $"UPDATE reservation SET rsrv_result = '완료' WHERE rsrv_no = {reservationNo}";
                        dbc.DCom.CommandText = updateReservationStatusQuery;
                        dbc.DCom.ExecuteNonQuery();

                        UpdateSeatState(dbc.DR["rsrv_sc"].ToString());

                        if (checkOutTime == null && DateTime.Now > reservationClosingTime)
                        {
                            DateTime warningDate = reservationClosingTime.AddSeconds(1);
                            InsertWarning(selectedMemberId, warningDate, "예약 마감 시간 초과");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating reservation status and inserting warning: {ex.Message}");
            }
        }

        // 경고 테이블에 데이터 추가
        private void InsertWarning(int selectedMemberId, DateTime warningDate, string warningContent)
        {
            try
            {
                string insertWarningQuery = "INSERT INTO warning (warning_no, member_no, warning_date, warning_content) " +
                                            $"VALUES (warning_no_seq.NEXTVAL, {Convert.ToInt32(selectedMemberId)}, TO_TIMESTAMP('{warningDate.ToString("yyyy-MM-dd HH:mm:ss")}', 'YYYY-MM-DD HH24:MI:SS'), '{warningContent}')";

                dbc.DCom.CommandText = insertWarningQuery;

                dbc.DCom.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inserting warning: {ex.Message}");
            }
        }

        // 예약 종류 테이블에서 예약 진행 시간을 가져옴
        private int GetReservationTypeDuration(string reservationTypeCode)
        {
            int duration = 0;

            try
            {
                dbc.DS.Clear();
                string query = "SELECT * FROM reservation_type";
                dbc.DCom.CommandText = query;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservation_type");

                DataRow[] rows = dbc.DS.Tables["reservation_type"].Select($"rsrv_tc = '{reservationTypeCode}'");

                if (rows.Length > 0)
                {
                    object result = rows[0]["rsrv_time"];
                    if (result != null && result != DBNull.Value)
                    {
                        duration = Convert.ToInt32(result);
                    }
                }
            }
            catch (DataException DE)
            {
                MessageBox.Show(DE.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching reservation type duration: {ex.Message}");
            }

            return duration;
        }

        // 회원 테이블에 회원 정보를 업데이트 - 경고횟수, 마지막경고날짜, 회원상태
        private void UpdateMemberInformation()
        {
            try
            {
                dbc.DS.Clear();
                string warningQuery = $"SELECT COUNT(*), MAX(warning_date) FROM warning WHERE member_no = {memberId}";
                dbc.DCom.CommandText = warningQuery;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "warning");
                dbc.WarningTable = dbc.DS.Tables["warning"];

                int warningCount = 0;
                DateTime? mostRecentWarningDate = null;
                memberState = "사용 가능";

                if (dbc.WarningTable.Rows.Count > 0)
                {
                    DataRow warningInfo = dbc.WarningTable.Rows[0];
                    warningCount = Convert.ToInt32(warningInfo[0]);

                    mostRecentWarningDate = DBNull.Value.Equals(warningInfo[1]) ? (DateTime?)null : Convert.ToDateTime(warningInfo[1]);

                    if (warningCount >= 5)
                    {
                        DateTime currentDate = DateTime.Now;

                        DateTime expirationDate = mostRecentWarningDate?.AddDays(30) ?? currentDate;

                        if (warningCount >= 10)
                        {
                            memberState = "사용 불가";
                        }
                        else
                        {
                            memberState = "한 달 불가";
                        }
                    }
                    else
                    {
                        memberState = "사용 가능";
                    }

                    string updateQuery = $"UPDATE member SET warning_count = {warningCount}, warning_ld = ";

                    if (mostRecentWarningDate.HasValue)
                    {
                        updateQuery += $"TO_TIMESTAMP('{mostRecentWarningDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}', 'YYYY-MM-DD HH24:MI:SS')";
                    }
                    else
                    {
                        updateQuery += "NULL";
                    }

                    updateQuery += $", member_state = '{memberState}' WHERE member_no = {memberId}";

                    dbc.DCom.CommandText = updateQuery;

                    if (dbc.Con.State == ConnectionState.Closed)
                    {
                        dbc.Con.Open();
                    }

                    dbc.DCom.ExecuteNonQuery();
                }

                SetControlAccessibility(memberState);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating member information: {ex.Message}");
            }
        }

        // 회원 정보 불러오기
        private void LoadMemberData()
        {
            dbc.DS.Clear();
            string query = $"SELECT * FROM member WHERE member_no = {memberId}";
            dbc.DCom.CommandText = query;
            dbc.DA.SelectCommand = dbc.DCom;
            dbc.DA.Fill(dbc.DS, "member");
            dbc.MemberTable = dbc.DS.Tables["member"];
            string rsrvQuery = $"SELECT * FROM reservation_type";
            dbc.DCom.CommandText = rsrvQuery;
            dbc.DA.SelectCommand = dbc.DCom;
            dbc.DA.Fill(dbc.DS, "reservation_type");

            try
            {
                DataRow member = dbc.MemberTable.Rows[0];
                metroLabel1.Text += " " + member["member_name"].ToString();
                metroLabel2.Text += " " + member["gender"].ToString();
                metroLabel3.Text += " " + member["membership_type"].ToString();
                metroLabel4.Text += " " + member["warning_count"].ToString();

                if (member["warning_ld"] == DBNull.Value)
                {
                    metroLabel5.Text += " 데이터 없음";
                }
                else
                {
                    metroLabel5.Text += " " + ((DateTime)member["warning_ld"]).ToString("yyyy-MM-dd HH:mm:ss");
                }

                metroLabel6.Text += " " + member["member_state"].ToString();

                memberState = member["member_state"].ToString();
                SetGroupBoxVisibility();

                string membershipType = member["membership_type"].ToString();

                if (dbc.DS.Tables.Contains("reservation_type"))
                {
                    FilterReservationTypes(membershipType);
                }
                else
                {
                    MessageBox.Show("Error: reservation_type table not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 가져오기 중 오류 발생: {ex.Message}");
            }
        }

        // 월회원이 사물함을 이미 선택했는지의 여부 확인
        private bool MemberHasSelectedLocker()
        {
            try
            {
                dbc.DS.Tables.Remove("member");
                string query = $"SELECT * FROM member WHERE member_no = {memberId}";
                dbc.DCom.CommandText = query;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "member");
                dbc.MemberTable = dbc.DS.Tables["member"];

                if (dbc.MemberTable != null)
                {
                    DataRow[] memberRows = dbc.MemberTable.Select($"member_no={memberId} AND membership_type='월회원' AND locker_code IS NOT NULL");
                    return memberRows.Length > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in MemberHasSelectedLocker: {ex.Message}");
            }

            return false;
        }

        // 월회원이 사물함을 이미 선택했을 경우, 그 사물함 번호를 highlight
        private void HighlightSelectedLocker()
        {
            try
            {
                dbc.DS.Tables.Remove("member");
                string query = $"SELECT * FROM member WHERE member_no = {memberId}";
                dbc.DCom.CommandText = query;
                dbc.DA.Fill(dbc.DS, "member");
                dbc.MemberTable = dbc.DS.Tables["member"];
                DataRow[] memberRows = dbc.MemberTable.Select($"membership_type = '월회원' AND locker_code IS NOT NULL");

                if (memberRows.Length > 0)
                {
                    string selectedLocker = memberRows[0]["locker_code"].ToString();

                    foreach (Control control in groupBox4.Controls)
                    {
                        if (control is MetroButton lockerButton)
                        {
                            string lockerCode = lockerButton.Name;

                            if (lockerCode == selectedLocker)
                            {
                                lockerButton.Enabled = true;
                                lockerButton.Highlight = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in HighlightSelectedLocker: {ex.Message}");
            }
        }

        // 멤버십 유형에 따라 예약 종류 리스트뷰에 뿌려줄 데이터를 달리 함
        private void FilterReservationTypes(string membershipType)
        {
            listView1.Items.Clear();
            metroTextBox2.Enabled = true;

            try
            {
                string commandString = "SELECT rsrv_tc, rsrv_name, rsrv_time, price FROM reservation_type";

                dbc.DCom.CommandText = commandString;
                dbc.DA.SelectCommand = dbc.DCom;

                dbc.DS.Clear();
                dbc.DA.Fill(dbc.DS, "reservation_type");

                using (DataTable reservationTypeTable = dbc.DS.Tables["reservation_type"])
                {
                    foreach (DataRow row in reservationTypeTable.Rows)
                    {
                        string rsrv_tc = row["rsrv_tc"].ToString();

                        if (membershipType == "월회원")
                        {
                            metroTextBox2.Text = "M";
                            metroTextBox2.Enabled = false;
                        }

                        if ((membershipType == "월회원" && rsrv_tc == "M") || (membershipType == "일반회원" && (rsrv_tc == "H1" || rsrv_tc == "H2" || rsrv_tc == "H4" || rsrv_tc == "H8" || rsrv_tc == "D")))
                        {
                            ListViewItem item = new ListViewItem(rsrv_tc);
                            item.SubItems.Add(row["rsrv_name"].ToString());
                            item.SubItems.Add(row["rsrv_time"].ToString());
                            item.SubItems.Add(row["price"].ToString());
                            listView1.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in FilterReservationTypes: {ex.Message}");
            }
        }

        // 회원 상태에 따라 시스템 사용 제한
        private void SetControlAccessibility(string memberState)
        {
            switch (memberState)
            {
                case "한 달 불가":
                case "사용 불가":
                    groupBox3.Enabled = false;
                    metroButton44.Enabled = false;
                    metroButton3.Enabled = false;
                    groupBox4.Enabled = false;
                    metroButton45.Enabled = false;
                    metroButton2.Enabled = false;
                    groupBox5.Enabled = false;
                    groupBox6.Enabled = false;
                    groupBox7.Enabled = false;
                    break;

                case "사용 가능":
                    groupBox3.Enabled = true;
                    metroButton44.Enabled = true;
                    metroButton3.Enabled = true;
                    groupBox4.Enabled = true;
                    metroButton45.Enabled = true;
                    metroButton2.Enabled = true;
                    groupBox5.Enabled = true;
                    groupBox6.Enabled = true;
                    groupBox7.Enabled = true;
                    break;
            }
        }

        // 월회원인데 사물함을 아직 선택하지 않은 경우, 사물함 검색, 선택 활성화
        private void SetGroupBoxVisibility()
        {
            dbc.DS.Tables.Remove("member");
            string query = $"SELECT * FROM member WHERE member_no = {memberId}";
            dbc.DCom.CommandText = query;
            dbc.DA.SelectCommand = dbc.DCom;
            dbc.DA.Fill(dbc.DS, "member");
            dbc.MemberTable = dbc.DS.Tables["member"];

            DataRow memberRow = dbc.MemberTable.Rows[0];
            string membershipType = memberRow["membership_type"].ToString();

            if (membershipType == "월회원")
            {
                bool memberHasSelectedLocker = MemberHasSelectedLocker();

                groupBox4.Enabled = true;
                metroButton45.Enabled = !memberHasSelectedLocker;
                metroButton2.Enabled = !memberHasSelectedLocker;

                if (memberHasSelectedLocker)
                {
                    HighlightSelectedLocker();
                }
            }
            else
            {
                groupBox4.Enabled = false;
                metroButton45.Enabled = false;
                metroButton2.Enabled = false;
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

        // 사물함 버튼 enabled false로 초기화
        private void InitializeLockerButtons()
        {
            foreach (Control control in groupBox4.Controls)
            {
                if (control is MetroButton lockerButton)
                {
                    lockerButton.Enabled = false;
                    lockerButton.Click += LockerButton_Click;
                }
            }
        }

        // 사물함 버튼 클릭 이벤트
        private void LockerButton_Click(object sender, EventArgs e)
        {
            if (lastClickedButton != null)
            {
                lastClickedButton.BackColor = SystemColors.Control;
                lastClickedButton.Highlight = false;
            }

            MetroButton clickedButton = (MetroButton)sender;
            clickedButton.BackColor = Color.LightBlue;
            clickedButton.Highlight = true;
            selectedLocker = clickedButton.Name;

            lastClickedButton = clickedButton;
        }

        // 회원 상태 - 상태에 따라 메뉴 제한을 위해 사용
        public string MemberState
        {
            get { return memberState; }
        }

        // 경고 내역 검색 버튼 클릭 이벤트
        private void metroButton43_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateReservationStatusAndInsertWarning();

                listView2.Items.Clear();

                string commandString = $"SELECT warning_no, warning_date, warning_content FROM warning WHERE member_no = {memberId}";

                dbc.DCom.CommandText = commandString;
                dbc.DA.SelectCommand = dbc.DCom;

                dbc.DS.Clear();
                dbc.DA.Fill(dbc.DS, "warning");

                using (DataTable warningTable = dbc.DS.Tables["warning"])
                {
                    foreach (DataRow row in warningTable.Rows)
                    {
                        ListViewItem item = new ListViewItem(row["warning_no"].ToString());
                        item.SubItems.Add(((DateTime)row["warning_date"]).ToString("yyyy-MM-dd HH:mm:ss"));
                        item.SubItems.Add(row["warning_content"].ToString());
                        listView2.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving data: {ex.Message}");
            }
        }

        // 사용이 가능한 좌석 검색 버튼 클릭 이벤트
        private void metroButton44_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateReservationStatusAndInsertWarning();

                dbc.DS.Clear();

                string memberQuery = $"SELECT * FROM member WHERE member_no = {memberId}";
                dbc.DCom.CommandText = memberQuery;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "member");
                dbc.MemberTable = dbc.DS.Tables["member"];

                string seatQuery = "SELECT * FROM seat";
                dbc.DCom.CommandText = seatQuery;
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

                                if (seatStatus == "사용 중")
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

        // 좌석 선택 버튼 클릭 이벤트 - 예약의 좌석코드 textbox에 값이 채워짐
        private void metroButton3_Click(object sender, EventArgs e)
        {
            bool seatSelected = false;
            foreach (Control control in groupBox3.Controls)
            {
                if (control is MetroButton seatButton && seatButton.BackColor == Color.LightBlue)
                {
                    seatSelected = true;
                    metroTextBox1.Text = seatButton.Name;
                    metroTextBox1.Enabled = false;
                }
            }
            if (!seatSelected)
            {
                MessageBox.Show("좌석을 선택하세요.");
            }
        }

        // 사용이 가능한 사물함 검색 버튼 클릭 이벤트
        private void metroButton45_Click(object sender, EventArgs e)
        {
            try
            {
                dbc.DS.Clear();
                dbc.DCom.CommandText = "SELECT * FROM locker";
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "locker");
                dbc.LockerTable = dbc.DS.Tables["locker"];

                InitializeLockerButtons();

                foreach (Control control in groupBox4.Controls)
                {
                    if (control is MetroButton lockerButton)
                    {
                        string lockerName = lockerButton.Name;
                        DataRow[] lockerRows = dbc.LockerTable.Select($"locker_code = '{lockerName}'");

                        if (lockerRows.Length > 0)
                        {
                            string lockerStatus = lockerRows[0]["locker_state"].ToString();

                            if (lockerStatus == "사용 중")
                            {
                                lockerButton.Enabled = false;
                            }
                            else
                            {
                                lockerButton.Enabled = true;
                            }
                        }
                        else
                        {
                            lockerButton.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in metroButton45_Click: {ex.Message}");
            }
        }

        // 월회원이 사물함 추가한 후에는 사물함 버튼들을 클릭해도 클릭이 되지 않도록 함
        private void DetachLockerButtonClickEvent()
        {
            foreach (Control control in groupBox4.Controls)
            {
                if (control is MetroButton lockerButton)
                {
                    lockerButton.Click -= LockerButton_Click;
                }
            }
        }

        // 사물함 선택 버튼 클릭 이벤트
        private void metroButton2_Click(object sender, EventArgs e)
        {
            bool lockerSelected = false;

            foreach (Control control in groupBox4.Controls)
            {
                if (control is MetroButton lockerButton && lockerButton.BackColor == Color.LightBlue)
                {
                    lockerSelected = true;
                    selectedLocker = lockerButton.Name;

                    DialogResult result = MessageBox.Show($"{selectedLocker.Substring(1)} 사물함을 사용하실 겁니까?", "사물함 선택 확인", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            dbc.DS.Clear();
                            dbc.DCom.CommandText = $"SELECT * FROM member WHERE member_no = {memberId}";
                            dbc.DA.SelectCommand = dbc.DCom;
                            dbc.DA.Fill(dbc.DS, "member");

                            DataRow[] memberRows = dbc.DS.Tables["member"].Select($"member_no = {memberId} AND membership_type = '월회원'");

                            if (memberRows.Length > 0)
                            {
                                memberRows[0]["locker_code"] = selectedLocker;
                            }
                            else
                            {
                                MessageBox.Show($"회원 정보를 찾을 수 없음: {memberId}");
                                return;
                            }

                            dbc.DA.UpdateCommand = dbc.MyCommandBuilder.GetUpdateCommand();
                            dbc.DA.InsertCommand = dbc.MyCommandBuilder.GetInsertCommand();
                            dbc.DA.DeleteCommand = dbc.MyCommandBuilder.GetDeleteCommand();

                            dbc.DA.Update(dbc.DS, "member");
                            dbc.DS.AcceptChanges();
                            MessageBox.Show("사물함 추가 완료!");

                            metroButton45.Enabled = false;
                            metroButton2.Enabled = false;

                            DetachLockerButtonClickEvent();
                            dbc.DS.Clear();
                            dbc.DCom.CommandText = "SELECT * FROM locker";
                            dbc.DA.SelectCommand = dbc.DCom;
                            dbc.DA.Fill(dbc.DS, "locker");
                            dbc.LockerTable = dbc.DS.Tables["locker"];

                            DataRow[] lockerRows = dbc.LockerTable.Select($"locker_code = '{selectedLocker}'");

                            if (lockerRows.Length > 0)
                            {
                                lockerRows[0]["locker_code"] = selectedLocker;
                                lockerRows[0]["locker_state"] = "사용 중";
                            }
                            else
                            {
                                MessageBox.Show($"사물함 정보를 찾을 수 없음: {selectedLocker}");
                                return;
                            }

                            dbc.DCom.Connection = dbc.Con;
                            dbc.DCom.CommandText = $"UPDATE LOCKER SET LOCKER_STATE = '사용 중', LOCKER_CODE = '{selectedLocker}' WHERE LOCKER_CODE = '{selectedLocker}'";
                            dbc.DCom.ExecuteNonQuery();
                        }
                        catch (DataException DE)
                        {
                            MessageBox.Show(DE.Message);
                        }
                        catch (Exception DE)
                        {
                            MessageBox.Show(DE.Message);
                        }
                    }
                }
            }

            if (!lockerSelected)
            {
                MessageBox.Show("사물함을 선택하세요.");
            }
        }

        // 예약 버튼 클릭 이벤트
        private void metroButton1_Click(object sender, EventArgs e)
        {
            UpdateReservationStatusAndInsertWarning();

            try
            {
                dbc.DS.Clear();
                dbc.DCom.CommandText = $"SELECT COUNT(*) FROM reservation WHERE member_no = {memberId} AND rsrv_result = '진행 중'";
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservation");
                int existingReservationCount = Convert.ToInt32(dbc.DCom.ExecuteScalar());

                if (existingReservationCount > 0)
                {
                    MessageBox.Show("이미 진행 중인 예약이 있습니다. 새로운 예약을 생성하기 전에 기존 예약을 완료하거나 취소해주세요.");
                    return;
                }

                dbc.DS.Clear();
                dbc.DCom.CommandText = "select * from reservation";
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservation");
                dbc.ReservationTable = dbc.DS.Tables["reservation"];

                decimal nextVal = dbc.GetNextSequenceValue("rsrv_no_seq");

                DataRow newRow = dbc.ReservationTable.NewRow();
                newRow["rsrv_no"] = nextVal;
                newRow["member_no"] = memberId;
                newRow["rsrv_sc"] = metroTextBox1.Text;
                newRow["rsrv_tc"] = metroTextBox2.Text;
                newRow["rsrv_st"] = DateTime.Now;
                newRow["payment_method"] = metroComboBox1.SelectedItem;
                newRow["rsrv_result"] = "진행 중";

                dbc.ReservationTable.Rows.Add(newRow);

                dbc.DA.UpdateCommand = dbc.MyCommandBuilder.GetUpdateCommand();
                dbc.DA.InsertCommand = dbc.MyCommandBuilder.GetInsertCommand();
                dbc.DA.DeleteCommand = dbc.MyCommandBuilder.GetDeleteCommand();

                dbc.DA.Update(dbc.DS, "reservation");
                dbc.DS.AcceptChanges();
                MessageBox.Show("예약 완료!");

                dbc.DS.Clear();
                dbc.DCom.CommandText = "select * from seat";
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "seat");
                dbc.SeatTable = dbc.DS.Tables["seat"];

                DataRow[] seatRows = dbc.SeatTable.Select($"seat_code = '{selectedSeat}'");

                if (seatRows.Length > 0)
                {
                    seatRows[0]["SEAT_CODE"] = selectedSeat;
                    seatRows[0]["SEAT_STATE"] = "사용 중";
                }
                else
                {
                    MessageBox.Show($"좌석 정보를 찾을 수 없음: {selectedSeat}");
                    return;
                }

                dbc.DCom.Connection = dbc.Con;
                dbc.DCom.CommandText = $"UPDATE SEAT SET SEAT_STATE = '사용 중', SEAT_CODE = '{selectedSeat}' WHERE SEAT_CODE = '{selectedSeat}'";
                dbc.DCom.ExecuteNonQuery();
            }
            catch (DataException DE)
            {
                MessageBox.Show(DE.Message);
            }
            catch (Exception DE)
            {
                MessageBox.Show(DE.Message);
            }
        }

        // 예약 내역 검색 버튼 클릭 이벤트
        private void metroButton5_Click(object sender, EventArgs e)
        {
            UpdateReservationStatusAndInsertWarning();

            try
            {
                listView3.Items.Clear();
                dbc.DS.Clear();
                dbc.DCom.CommandText = $"SELECT r.rsrv_no, r.rsrv_sc, r.rsrv_tc, r.rsrv_st, rt.rsrv_time, r.payment_method, r.rsrv_result, m.check_out_time " +
                    $"FROM reservation r " +
                    $"JOIN reservation_type rt ON r.rsrv_tc = rt.rsrv_tc " +
                    $"LEFT JOIN member_check_in_out m ON r.rsrv_no = m.rsrv_no " +
                    $"WHERE r.member_no = {memberId}";
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservations");

                foreach (DataRow row in dbc.DS.Tables["reservations"].Rows)
                {
                    DateTime reservationStartTime = Convert.ToDateTime(row["rsrv_st"]);
                    int reservationTime = Convert.ToInt32(row["rsrv_time"]);
                    DateTime reservationClosingTime;

                    if (row["check_out_time"] != DBNull.Value)
                    {
                        reservationClosingTime = Convert.ToDateTime(row["check_out_time"]);
                    }
                    else
                    {
                        reservationClosingTime = reservationStartTime.AddHours(reservationTime);
                    }

                    ListViewItem item = new ListViewItem(row["rsrv_no"].ToString());
                    item.SubItems.Add(row["rsrv_sc"].ToString());
                    item.SubItems.Add(row["rsrv_tc"].ToString());
                    item.SubItems.Add(reservationStartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    item.SubItems.Add(reservationClosingTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    item.SubItems.Add(row["payment_method"].ToString());
                    item.SubItems.Add(row["rsrv_result"].ToString());
                    listView3.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 가져오기 중 오류 발생: {ex.Message}");
            }
        }

        // 예약 종류 리스트뷰에서 항목을 선택하면(해당 줄 아무 곳이나), 예약의 예약종류코드 textbox에 값이 채워짐
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = listView1.HitTest(e.Location);

            if (hit.Item != null)
            {
                hit.Item.Selected = true;

                metroTextBox2.Text = hit.Item.SubItems[0].Text;
                metroTextBox2.Enabled = false;
            }
        }
    }
}
