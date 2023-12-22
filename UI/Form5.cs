/*
 * 작성자 : 남기연
 * 폼 설명 : 나의 정보 관리; 로그인한 회원 정보 수정, 삭제 가능
*/

using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UI
{
    public partial class Form5 : MetroFramework.Forms.MetroForm
    {
        DBClass dbc = new DBClass();
        CommonUtil com = new CommonUtil();

        private int memberId;
        private PictureBox eyePictureBox1;
        private PictureBox eyePictureBox2;

        public Form5(int memberId)
        {
            InitializeComponent();
            dbc.DB_ObjCreate();
            dbc.DB_Open();
            dbc.DB_Access();
            AddEyeIconControls();
            this.memberId = memberId;
            metroLabel2.Visible = false;
            metroLabel3.Visible = false;
            metroLabel4.Visible = false;
            metroLabel5.Visible = false;
            metroLabel6.Visible = false;
            metroLabel7.Visible = false;
            metroLabel8.Visible = false;
            metroTextBox2.Visible = false;
            metroTextBox3.Visible = false;
            metroTextBox4.Visible = false;
            metroTextBox5.Visible = false;
            metroComboBox1.Visible = false;
            dateTimePicker1.Visible = false;
            metroComboBox2.Visible = false;
            metroButton1.Visible = false;
            eyePictureBox1.Visible = false;
            eyePictureBox2.Visible = false;
            metroButton2.Visible = false;

            dbc.DS.Clear();
            dbc.DCom.CommandText = $"SELECT * FROM member WHERE member_no = {memberId}";
            dbc.DA.SelectCommand = dbc.DCom;
            dbc.DA.Fill(dbc.DS, "member");
            dbc.MemberTable = dbc.DS.Tables["member"];

            try
            {
                DataRow member = dbc.MemberTable.Rows[0];
                metroTextBox2.Text = member["member_name"].ToString();
                metroTextBox3.Text = member["phone_number"].ToString();
                metroTextBox4.Text = member["password"].ToString();
                metroTextBox5.Text = member["password"].ToString();
                metroComboBox1.SelectedItem = member["gender"].ToString();
                dateTimePicker1.Value = DateTime.Parse(member["birth_date"].ToString());
                metroComboBox2.SelectedItem = member["membership_type"].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 가져오기 중 오류 발생: {ex.Message}");
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
            metroLabel2.Visible = true;
            metroLabel3.Visible = true;
            metroLabel4.Visible = true;
            metroLabel5.Visible = true;
            metroLabel6.Visible = true;
            metroLabel7.Visible = true;
            metroLabel8.Visible = true;
            metroTextBox2.Visible = true;
            metroTextBox3.Visible = true;
            metroTextBox4.Visible = true;
            metroTextBox5.Visible = true;
            metroComboBox1.Visible = true;
            dateTimePicker1.Visible = true;
            metroComboBox2.Visible = true;
            metroButton1.Visible = true;
            eyePictureBox1.Visible = true;
            eyePictureBox2.Visible = true;
            metroButton2.Visible = true;

            metroTextBox1.Enabled = false;
            metroLabel1.Enabled = false;
            metroButton4.Enabled = false;
        }

        // 핀 입력 옆 눈 아이콘 추가
        private void AddEyeIconControls()
        {
            eyePictureBox1 = new PictureBox();
            eyePictureBox1.Image = Properties.Resources.eye_closed;
            eyePictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            eyePictureBox1.Size = new Size(20, 20);
            eyePictureBox1.Cursor = Cursors.Hand;
            eyePictureBox1.Click += eyePictureBox1_Click;

            eyePictureBox1.Location = new Point(metroTextBox4.Right, metroTextBox4.Top + (metroTextBox4.Height - eyePictureBox1.Height) / 2);

            this.Controls.Add(eyePictureBox1);

            eyePictureBox2 = new PictureBox();
            eyePictureBox2.Image = Properties.Resources.eye_closed;
            eyePictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            eyePictureBox2.Size = new Size(20, 20);
            eyePictureBox2.Cursor = Cursors.Hand;
            eyePictureBox2.Click += eyePictureBox2_Click;

            eyePictureBox2.Location = new Point(metroTextBox5.Right, metroTextBox5.Top + (metroTextBox5.Height - eyePictureBox2.Height) / 2);

            this.Controls.Add(eyePictureBox2);
        }

        // 핀 입력 옆 눈 아이콘 클릭 이벤트
        private void eyePictureBox1_Click(object sender, EventArgs e)
        {
            metroTextBox4.UseSystemPasswordChar = !metroTextBox4.UseSystemPasswordChar;
            eyePictureBox1.Image = metroTextBox4.UseSystemPasswordChar ? Properties.Resources.eye_closed : Properties.Resources.eye_open;
            metroTextBox4.PasswordChar = metroTextBox4.UseSystemPasswordChar ? '\u25CF' : '\0';
        }

        // 핀 확인 입력 옆 눈 아이콘 클릭 이벤트
        private void eyePictureBox2_Click(object sender, EventArgs e)
        {
            metroTextBox5.UseSystemPasswordChar = !metroTextBox5.UseSystemPasswordChar;
            eyePictureBox2.Image = metroTextBox5.UseSystemPasswordChar ? Properties.Resources.eye_closed : Properties.Resources.eye_open;
            metroTextBox5.PasswordChar = metroTextBox5.UseSystemPasswordChar ? '\u25CF' : '\0';
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

        // 나의 정보 관리 폼 닫기 버튼 클릭 이벤트
        private void metroButton3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 회원 탈퇴 버튼 클릭 이벤트
        private void metroButton2_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("탈퇴하시겠습니까?", "회원 탈퇴", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    dbc.DS.Clear();

                    dbc.DCom.CommandText = $"SELECT * FROM member WHERE member_no = {memberId}";
                    dbc.DA.SelectCommand = dbc.DCom;
                    dbc.DA.Fill(dbc.DS, "member");
                    dbc.MemberTable = dbc.DS.Tables["member"];

                    dbc.DCom.CommandText = $"SELECT * FROM reservation";
                    dbc.DA.SelectCommand = dbc.DCom;
                    dbc.DA.Fill(dbc.DS, "reservation");
                    dbc.ReservationTable = dbc.DS.Tables["reservation"];

                    dbc.DCom.CommandText = $"SELECT * FROM locker";
                    dbc.DA.SelectCommand = dbc.DCom;
                    dbc.DA.Fill(dbc.DS, "locker");
                    dbc.LockerTable = dbc.DS.Tables["locker"];

                    DataRow[] activeReservationRows = dbc.DS.Tables["reservation"].Select($"rsrv_result = '진행 중' AND member_no = {memberId}");

                    if (activeReservationRows.Length > 0)
                    {
                        MessageBox.Show("진행 중인 예약이 있어 회원 정보를 삭제할 수 없습니다.");
                        return;
                    }

                    DataRow[] memberRows = dbc.DS.Tables["member"].Select($"member_no = {memberId}");
                    if (memberRows.Length > 0 && memberRows[0]["membership_type"].ToString() == "월회원")
                    {
                        DataRow[] membermRows = dbc.DS.Tables["member"].Select($"member_no = {memberId} and membership_type='월회원' and locker_code is not null");
                        if (membermRows.Length > 0)
                        {
                            string selectedLocker = membermRows[0]["locker_code"].ToString();
                            string updateLockerQuery = $"UPDATE locker SET locker_state = '사용 가능' WHERE locker_code = '{selectedLocker}'";

                            dbc.DCom.CommandText = updateLockerQuery;
                            dbc.DCom.ExecuteNonQuery();
                        }
                    }
                    string deleteMemberQuery = $"DELETE FROM member WHERE member_no = {memberId}";

                    dbc.DCom.CommandText = deleteMemberQuery;
                    dbc.DCom.ExecuteNonQuery();

                    MessageBox.Show("회원 정보 삭제 완료!");

                    Form3 form3 = Application.OpenForms.OfType<Form3>().FirstOrDefault();
                    if (form3 != null)
                    {
                        form3.Close();
                    }

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during member information update: {ex.Message}");
            }
        }

        // 나의 정보 저장 버튼 클릭 이벤트
        private void metroButton1_Click(object sender, EventArgs e)
        {
            try
            {
                dbc.DS.Clear();

                dbc.DCom.CommandText = $"SELECT * FROM member WHERE member_no = {memberId}";
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "member");
                dbc.MemberTable = dbc.DS.Tables["member"];

                dbc.DCom.CommandText = $"SELECT * FROM reservation";
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "reservation");
                dbc.ReservationTable = dbc.DS.Tables["reservation"];

                dbc.DCom.CommandText = $"SELECT * FROM locker";
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "locker");
                dbc.LockerTable = dbc.DS.Tables["locker"];

                DataRow[] activeReservationRows = dbc.DS.Tables["reservation"].Select($"rsrv_result = '진행 중' AND member_no = {memberId}");

                if (activeReservationRows.Length > 0)
                {
                    MessageBox.Show("진행 중인 예약이 있어 회원 정보를 수정할 수 없습니다.");
                    return;
                }

                if (!com.IsValidPhoneNumberFormat(metroTextBox3.Text))
                {
                    MessageBox.Show("유효하지 않은 전화번호입니다. 전화번호는 11자리의 숫자로 이루어져야 합니다.");
                    return;
                }

                if (!com.IsValidPasswordFormat(metroTextBox4.Text))
                {
                    MessageBox.Show("유효하지 않은 핀 번호입니다. 핀 번호는 4자리의 숫자로 이루어져야 합니다.");
                    return;
                }

                if (metroTextBox4.Text == metroTextBox5.Text)
                {
                    if (metroComboBox2.SelectedItem.ToString() == "일반회원")
                    {
                        DataRow[] membermRows = dbc.DS.Tables["member"].Select($"member_no = {memberId} and membership_type='월회원'");

                        if (membermRows.Length > 0)
                        {
                            if (membermRows[0]["locker_code"] != DBNull.Value)
                            {
                                string selectedLocker = membermRows[0]["locker_code"].ToString();
                                membermRows[0]["locker_code"] = DBNull.Value;
                                dbc.DA.Update(dbc.DS, "member");
                                dbc.DS.AcceptChanges();

                                string updateLockerQuery = $"UPDATE locker SET locker_state = '사용 가능' WHERE locker_code = '{selectedLocker}'";
                                
                                dbc.DCom.CommandText = updateLockerQuery;
                                dbc.DCom.ExecuteNonQuery();
                            }
                        }
                    }
                    DataRow[] memberRows = dbc.DS.Tables["member"].Select($"member_no = {memberId}");
                    memberRows[0]["member_name"] = metroTextBox2.Text;
                    memberRows[0]["phone_number"] = metroTextBox3.Text;
                    memberRows[0]["password"] = Convert.ToInt32(metroTextBox4.Text);
                    memberRows[0]["gender"] = metroComboBox1.SelectedItem.ToString();
                    memberRows[0]["birth_date"] = dateTimePicker1.Value.Date;
                    memberRows[0]["membership_type"] = metroComboBox2.SelectedItem.ToString();
                    memberRows[0]["locker_code"] = DBNull.Value;
                    dbc.DA.Update(dbc.DS, "member");
                    dbc.DS.AcceptChanges();
                    MessageBox.Show("회원 정보 수정 완료!");

                    Form3 form3 = Application.OpenForms.OfType<Form3>().FirstOrDefault();
                    if (form3 != null)
                    {
                        form3.Close();
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show("핀 번호가 일치하지 않습니다.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during member information update: {ex.Message}");
            }
        }
    }
}
