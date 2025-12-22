using System.Text;

public static class SpriteTextUtil {
    /// <summary>
    /// 將文字轉成 <sprite name="x"> 格式的字串
    /// </summary>
    public static string GetSpriteNameTxt(string txt) {
        if (string.IsNullOrEmpty(txt)) return string.Empty;

        StringBuilder sb = new StringBuilder();
        foreach (char c in txt) {
            sb.Append($"<sprite name=\"{c}\">");
        }
        return sb.ToString();
    }
}
