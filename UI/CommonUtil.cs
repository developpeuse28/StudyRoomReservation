namespace UI
{
    public class CommonUtil
    {
        // 유효한 연락처 형식인지 확인 (숫자 11자리)
        public bool IsValidPhoneNumberFormat(string phoneNumber)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\d{11}$");
        }

        // 유효한 핀 형식인지 확인 (숫자 4자리)
        public bool IsValidPasswordFormat(string password)
        {
            int dummy;
            return password.Length == 4 && int.TryParse(password, out dummy);
        }
    }
}
