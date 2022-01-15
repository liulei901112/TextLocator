using System.ComponentModel;

namespace TextLocator.Enums
{
    /// <summary>
    /// 枚举扩展
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// 获取枚举Description注解
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this System.Enum value)
        {
            if (null == value)
            {
                return null;
            }
            System.Reflection.FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            return attribArray.Length == 0 ? value.ToString() : (attribArray[0] as DescriptionAttribute).Description;
        }
    }
}
