/*
 * 작성자 : 남기연
 * 폼 설명 : 회원가입; 연락처 형식은 숫자 11자리, 핀 형식은 숫자 4자리
*/

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace UI
{
    public partial class Form2 : MetroFramework.Forms.MetroForm
    {
        DBClass dbc = new DBClass();
        CommonUtil com = new CommonUtil();

        private PictureBox eyePictureBox1;
        private PictureBox eyePictureBox2;

        public Form2()
        {
            InitializeComponent();
            dbc.DB_ObjCreate();
            dbc.DB_Open();
            dbc.DB_Access();

            AddEyeIconControls();
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

            eyePictureBox1.Location = new Point(metroTextBox3.Right, metroTextBox3.Top + (metroTextBox3.Height - eyePictureBox1.Height) / 2);

            this.Controls.Add(eyePictureBox1);

            eyePictureBox2 = new PictureBox();
            eyePictureBox2.Image = Properties.Resources.eye_closed;
            eyePictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            eyePictureBox2.Size = new Size(20, 20);
            eyePictureBox2.Cursor = Cursors.Hand;
            eyePictureBox2.Click += eyePictureBox2_Click;

            eyePictureBox2.Location = new Point(metroTextBox4.Right, metroTextBox4.Top + (metroTextBox4.Height - eyePictureBox2.Height) / 2);

            this.Controls.Add(eyePictureBox2);
        }

        // 핀 입력 옆 눈 아이콘 클릭 이벤트
        private void eyePictureBox1_Click(object sender, EventArgs e)
        {
            metroTextBox3.UseSystemPasswordChar = !metroTextBox3.UseSystemPasswordChar;
            eyePictureBox1.Image = metroTextBox3.UseSystemPasswordChar ? Properties.Resources.eye_closed : Properties.Resources.eye_open;
            metroTextBox3.PasswordChar = metroTextBox3.UseSystemPasswordChar ? '\u25CF' : '\0';
        }

        // 핀 확인 입력 옆 눈 아이콘 클릭 이벤트

        private void eyePictureBox2_Click(object sender, EventArgs e)
        {
            metroTextBox4.UseSystemPasswordChar = !metroTextBox4.UseSystemPasswordChar;
            eyePictureBox2.Image = metroTextBox4.UseSystemPasswordChar ? Properties.Resources.eye_closed : Properties.Resources.eye_open;
            metroTextBox4.PasswordChar = metroTextBox4.UseSystemPasswordChar ? '\u25CF' : '\0';
        }

        // 회원가입 완료 버튼 클릭 이벤트
        private void metroButton1_Click(object sender, EventArgs e)
        {
            try
            {
                dbc.DS.Clear();
                string query = "SELECT * FROM member";
                dbc.DCom.CommandText = query;
                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "member");

                string sequenceName = "member_no_seq";
                decimal nextVal = dbc.GetNextSequenceValue(sequenceName);

                DataRow newRow = dbc.DS.Tables["member"].NewRow();
                newRow["member_no"] = nextVal;
                newRow["member_name"] = metroTextBox1.Text;
                newRow["phone_number"] = metroTextBox2.Text;
                newRow["password"] = Convert.ToInt32(metroTextBox3.Text);
                newRow["gender"] = metroComboBox1.SelectedItem;
                newRow["birth_date"] = dateTimePicker1.Value.ToString("yyyy-MM-dd");
                newRow["membership_type"] = metroComboBox2.SelectedItem;
                newRow["locker_code"] = DBNull.Value;

                newRow["warning_count"] = 0;
                newRow["warning_ld"] = DBNull.Value;
                newRow["member_state"] = "사용 가능";

                if (!com.IsValidPhoneNumberFormat(metroTextBox2.Text))
                {
                    MessageBox.Show("유효하지 않은 전화번호입니다. 전화번호는 11자리의 숫자로 이루어져야 합니다.");
                    return;
                }

                if (!com.IsValidPasswordFormat(metroTextBox3.Text))
                {
                    MessageBox.Show("유효하지 않은 핀 번호입니다. 핀 번호는 4자리의 숫자로 이루어져야 합니다.");
                    return;
                }

                if (metroTextBox3.Text == metroTextBox4.Text)
                {
                    dbc.DS.Tables["member"].Rows.Add(newRow);
                    dbc.DA.Update(dbc.DS, "member");
                    dbc.DS.AcceptChanges();
                    MessageBox.Show("회원가입 완료!");
                    Close();
                }
                else
                {
                    MessageBox.Show("핀 번호가 일치하지 않습니다.");
                    return;
                }
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

        // 취소 버튼 클릭 이벤트
        private void metroButton2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
