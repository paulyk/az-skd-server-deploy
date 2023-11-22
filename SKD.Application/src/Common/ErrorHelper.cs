using SKD.Application.Common;

namespace SKD.Service;

public class ErrorHelper {
    public static Error Create<T>(Expression<Func<T, object>> expression, string msg) {

        string path;
        if (expression.Body is MemberExpression expression1) {
            path = expression1.Member.Name;
        } else {
            var op = ((UnaryExpression)expression.Body).Operand;
            path = ((MemberExpression)op).Member.Name;
        }

        return new Error() {
            Path = new List<string> { path },
            Message = msg
        };
    }
}
