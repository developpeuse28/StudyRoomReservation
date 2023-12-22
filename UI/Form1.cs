/*
 * 작성자 : 남기연
 * 폼 설명 : 회원가입 후, 아이디(연락처)와 핀으로 로그인
*/

using System;
using System.Data;
using System.Windows.Forms;

namespace UI
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        DBClass dbc = new DBClass();
        CommonUtil com = new CommonUtil();

        public Form1()
        {
            InitializeComponent();
            dbc.DB_ObjCreate();
            dbc.DB_Open();
            dbc.DB_Access();
        }

        // 로그인 버튼 클릭 이벤트
        private void metroButton1_Click(object sender, EventArgs e)
        {
            try
            {
                dbc.DS.Clear();

                if (!com.IsValidPhoneNumberFormat(metroTextBox1.Text))
                {
                    MessageBox.Show("유효하지 않은 전화번호 형식입니다. 전화번호는 11자리의 숫자로 이루어져야 합니다.");
                    return;
                }

                if (!com.IsValidPasswordFormat(metroTextBox2.Text))
                {
                    MessageBox.Show("유효하지 않은 핀 번호 형식입니다. 핀 번호는 4자리의 숫자로 이루어져야 합니다.");
                    return;
                }

                string query = "SELECT * FROM member WHERE phone_number = '" + metroTextBox1.Text +  "' AND password = '" + Convert.ToInt32(metroTextBox2.Text) + "'";

                dbc.DCom.CommandText = query;

                dbc.DA.SelectCommand = dbc.DCom;
                dbc.DA.Fill(dbc.DS, "member");

                if (dbc.DS.Tables["member"].Rows.Count > 0)
                {
                    int memberId = Convert.ToInt32(dbc.DS.Tables["member"].Rows[0]["member_no"]);

                    Form3 form3 = new Form3(memberId);
                    form3.ShowDialog();

                    metroTextBox1.Clear();
                    metroTextBox2.Clear();
                }
                else
                {
                    MessageBox.Show("로그인 실패. 아이디 또는 핀이 올바르지 않습니다.");
                }
            }
            catch (DataException DE)
            {
                MessageBox.Show(DE.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (dbc.Con.State == ConnectionState.Open)
                {
                    dbc.Con.Close();
                }
            }
        }

        // 회원가입 버튼 클릭 이벤트
        private void metroButton2_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }
    }
}
