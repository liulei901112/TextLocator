using System;
using System.Collections.Generic;
using System.Linq;
using TextLocator.Core;
using TextLocator.Entity;

namespace TextLocator.Util
{
    /// <summary>
    /// 区域工具类
    /// </summary>
    public class AreaUtil
    {
        /// <summary>
        /// 区域配置
        /// </summary>
        private static string AreaConfig = AppConst.AREA_CONFIG_KEY;
        /// <summary>
        /// 区域名称
        /// </summary>
        private static string AreaName = "AreaName";
        /// <summary>
        /// 区域文件夹
        /// </summary>
        private static string AreaFolders = "AreaFolders";
        /// <summary>
        /// 区域文件类型
        /// </summary>
        private static string AreaFileTypes = "AreaFileTypes";

        /// <summary>
        /// 私有构造方法
        /// </summary>
        private AreaUtil() { }

        /// <summary>
        /// 获取区域信息列表
        /// </summary>
        /// <returns></returns>
        public static List<AreaInfo> GetAreaInfoList()
        {
            List<AreaInfo> areaInfoList = new List<AreaInfo>();

            List<string> areaList = AppUtil.ReadSectionList(AreaConfig);
            if (areaList != null)
            {
                foreach (string areaId in areaList)
                {
                    // 根据区域获取明细配置
                    bool isEnable = bool.Parse(AppUtil.ReadValue(AreaConfig, areaId, "False"));
                    // 区域显示名称
                    string areaName = AppUtil.ReadValue(areaId, AreaName, "区域名称");
                    // 区域文件夹
                    string folders = AppUtil.ReadValue(areaId, AreaFolders, "");
                    // 区域文件类型
                    string fileTypeNames = AppUtil.ReadValue(areaId, AreaFileTypes, "");

                    // 区域文件夹解析
                    List<string> areaFolders = new List<string>();
                    if (!string.IsNullOrEmpty(folders))
                    {
                        areaFolders = folders.Split(',').ToList();
                    }
                    // 区域文件类型解析
                    List<Enums.FileType> areaFileTypes = new List<Enums.FileType>();
                    if (!string.IsNullOrEmpty(fileTypeNames))
                    {
                        List<string> tmps = fileTypeNames.Split(',').ToList();
                        foreach (string tmp in tmps)
                        {
                            areaFileTypes.Add((Enums.FileType)System.Enum.Parse(typeof(Enums.FileType), tmp));
                        }
                    }
                    
                    // 构造数据对象
                    AreaInfo areaInfo = new AreaInfo()
                    {
                        IsEnable = isEnable,
                        AreaId = areaId,
                        AreaName = areaName,
                        AreaFolders = areaFolders,
                        AreaFileTypes = areaFileTypes
                    };
                    // 全部搜索区
                    areaInfoList.Add(areaInfo);
                }
            }
            // 区域信息列表为空（填充默认区域信息列表）
            if (areaInfoList.Count <= 0)
            {
                AreaInfo areaInfo = new AreaInfo()
                {
                    IsEnable = true,
                    AreaId = "AreaDefault",
                    AreaName = "默认区域",
                    AreaFolders = new string[] { Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Environment.GetFolderPath(Environment.SpecialFolder.Desktop) }.ToList(),
                    AreaFileTypes = FileTypeUtil.GetFileTypesNotAll()
                };
                areaInfoList.Add(areaInfo);
            }
            // 保存信息
            foreach (AreaInfo areaInfo in areaInfoList)
            {
                // 区域根信息
                AppUtil.WriteValue(AreaConfig, areaInfo.AreaId, areaInfo.IsEnable + "");

                // 区域配置
                AppUtil.WriteValue(areaInfo.AreaId, AreaName, areaInfo.AreaName);
                AppUtil.WriteValue(areaInfo.AreaId, AreaFolders, string.Join(",", areaInfo.AreaFolders.ToArray()));
            }
            return areaInfoList;
        }

        /// <summary>
        /// 获取启用的区域信息列表
        /// </summary>
        /// <returns></returns>
        public static List<AreaInfo> GetEnableAreaInfoList()
        {
            // 获取全部区域信息列表
            List<AreaInfo> allAreaInfoList = GetAreaInfoList();
            return allAreaInfoList.Where(areaInfo => areaInfo.IsEnable).ToList();
        }

        /// <summary>
        /// 获取禁用的区域信息列表
        /// </summary>
        /// <returns></returns>
        public static List<AreaInfo> GetDisableAreaInfoList()
        {
            // 获取全部区域信息列表
            List<AreaInfo> allAreaInfoList = GetAreaInfoList();
            return allAreaInfoList.Where(areaInfo => !areaInfo.IsEnable).ToList();
        }

        /// <summary>
        /// 保存区域信息
        /// </summary>
        /// <param name="areaInfo"></param>
        public static void SaveAreaInfo(AreaInfo areaInfo)
        {
            // 区域名称
            AppUtil.WriteValue(areaInfo.AreaId, AreaName, areaInfo.AreaName);
            // 区域文件夹
            AppUtil.WriteValue(areaInfo.AreaId, AreaFolders, areaInfo.AreaFolders != null ? string.Join(",", areaInfo.AreaFolders.ToArray()) : null);
            // 区域文件类型
            AppUtil.WriteValue(areaInfo.AreaId, AreaFileTypes, areaInfo.AreaFileTypes != null ? string.Join(",", areaInfo.AreaFileTypes.ToArray()) : null);

            // 区域列表
            AppUtil.WriteValue(AreaConfig, areaInfo.AreaId, areaInfo.IsEnable + "");
        }

        /// <summary>
        /// 删除区域信息
        /// </summary>
        /// <param name="areaInfo"></param>
        public static void DeleteAreaInfo(AreaInfo areaInfo)
        {
            // 区域名称
            AppUtil.WriteValue(areaInfo.AreaId, AreaName, null);
            // 区域文件夹
            AppUtil.WriteValue(areaInfo.AreaId, AreaFolders, null);
            // 区域文件类型
            AppUtil.WriteValue(areaInfo.AreaId, AreaFileTypes, null);

            // 区域列表
            AppUtil.WriteValue(AreaConfig, areaInfo.AreaId, null);
        }

        /// <summary>
        /// 获取全部区域文件夹列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAreaFolderList()
        {
            List<string> areaFolderList = new List<string>();

            List<string> areaList = AppUtil.ReadSectionList(AreaConfig);
            if (areaList != null)
            {
                foreach (string areaId in areaList)
                {
                    // 区域文件夹
                    string folders = AppUtil.ReadValue(areaId, AreaFolders, "");
                    List<string> areaFolders = new List<string>();
                    if (!string.IsNullOrEmpty(folders))
                    {
                        areaFolders = folders.Split(',').ToList();
                    }
                    areaFolderList.AddRange(areaFolders);
                }
            }
            return areaFolderList;
        }

        /// <summary>
        /// 获取不包含自己的区域名称列表
        /// </summary>
        /// <param name="areaInfo">区域信息</param>
        /// <returns></returns>
        public static List<string> GetAreaNameListRuleOut(AreaInfo areaInfo)
        {
            List<string> areaNameList = new List<string>();
            List<string> areaList = AppUtil.ReadSectionList(AreaConfig);
            if (areaList != null)
            {
                foreach (string areaId in areaList)
                {
                    // 跳过区域ID相同的
                    if (areaId.Equals(areaInfo.AreaId))
                    {
                        continue;
                    }
                    // 区域显示名称
                    string areaName = AppUtil.ReadValue(areaId, AreaName, "区域名称");
                    // 加入区域名称列表
                    areaNameList.Add(areaName);
                }
            }
            return areaNameList;
        }
    }
}
