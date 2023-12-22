/*
 * 작성자 : 남기연
 * 폼 설명 : 메인 화면과 각종 나의 정보 관리 화면, 예약 관리 화면, 입퇴실 화면을 담는 틀
*/

using System;
using System.Windows.Forms;

namespace UI
{
    public partial class Form3 : Form
    {
        private int memberId;
        public Form3(int memberId)
        {
            InitializeComponent();
            this.memberId = memberId;
        }
        Form4 main;
        Form5 mypage;
        Form6 rsrv;
        Form7 checkio;

        private void Form3_Load(object sender, EventArgs e)
        {
            main = new Form4(memberId, this);
            main.MdiParent = this;
            main.Show();
        }

        // 나의 정보 관리 메뉴 클릭 이벤트
        private void 나의정보관리ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (main != null && main.MemberState == "사용 가능")
            {
                if (mypage == null || mypage.IsDisposed)
                {
                    mypage = new Form5(memberId);
                    mypage.MdiParent = this;
                    mypage.Show();
                }
            }
            else
            {
                MessageBox.Show("서비스를 이용할 수 없습니다.");
            }
        }

        // 예약 관리 메뉴 클릭 이벤트
        private void 예약관리ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (main != null && main.MemberState == "사용 가능")
            {
                if (rsrv == null || rsrv.IsDisposed)
                {
                    rsrv = new Form6(memberId);
                    rsrv.MdiParent = this;
                    rsrv.Show();
                }
            }
            else
            {
                MessageBox.Show("서비스를 이용할 수 없습니다.");
            }
        }

        // 입퇴실 메뉴 클릭 이벤트
        private void 입퇴실ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (main != null && main.MemberState == "사용 가능")
            {
                if (checkio == null || checkio.IsDisposed)
                {
                    checkio = new Form7(memberId);
                    checkio.MdiParent = this;
                    checkio.Show();
                }
            }
            else
            {
                MessageBox.Show("서비스를 이용할 수 없습니다.");
            }
        }

        // 로그아웃 메뉴 클릭 이벤트
        private void 로그아웃ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
