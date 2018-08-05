using System;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace abstractplay
{
    public class MyModificationCommandComparer : ModificationCommandComparer
    {
        protected override Func<object, object, int> GetComparer(Type type)
        {
            if (type == typeof(byte[]))
            {
                return (lhs, rhs) =>
                {
                    return this.CompareBytes((byte[])lhs, (byte[])rhs);
                };
            }
            else
            {
                return base.GetComparer(type);
            }
        }

        public int CompareBytes(byte[] lhs, byte[] rhs)
        {
            for (var i = 0; i < lhs.Length && i < rhs.Length; i++)
            {
                if (lhs[i] != rhs[i])
                {
                    return lhs[i] - rhs[i];
                }
            }
            return lhs.Length - rhs.Length;
        }
    }
}
