
public enum Operator
{
    Atom = 0, Plus, Minus, Times, Div
}
struct Expression
{
    public int A;
    public int B;
    public uint len; //式 A oper B の項数の上界
    public Operator oper;
    public bool isRightFromSeven; //isMidFromSeven
    public Expression(bool isSeven)
    {
        oper = Operator.Atom;
        A = isSeven ? 7 : 10;
        B = 0;
        len = 1;
    }
    public Expression(Operator oper, int A, int B, bool isRightFromSeven, uint len = uint.MaxValue / 2)
    {
        this.oper = oper;
        this.A = A;
        this.B = B;
        this.isRightFromSeven = isRightFromSeven;
        this.len = len;
    }
    public override string ToString()
    {
        return $"{oper} {A} {B} | {isRightFromSeven} -- {len}";
    }
}