// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and relevant international regulations.
//   使用本项目须严格遵守相关法律法规及开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//   本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//   This project is dual-licensed under the MIT License and Apache License 2.0,
//   完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//   please refer to the LICENSE file in the root of the source tree for the full license text.
//   禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//   It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//   侵犯他人合法权益等法律法规所禁止的行为！
//   or infringe the legitimate rights and interests of others, as prohibited by laws and regulations!
//   因基于本项目二次开发所产生的一切法律纠纷与责任，
//   Any disputes or liabilities arising from secondary development based on this project
//   本项目组织及贡献者概不承担。
//   shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//   GitHub 仓库：https://github.com/GameFrameX
//   GitHub Repository: https://github.com/GameFrameX
//   Gitee  仓库：https://gitee.com/GameFrameX
//   Gitee Repository:  https://gitee.com/GameFrameX
//   CNB  仓库：https://cnb.cool/GameFrameX
//   CNB Repository:    https://cnb.cool/GameFrameX
//   官方文档：https://gameframex.doc.alianblank.com/
//   Official Documentation: https://gameframex.doc.alianblank.com/
//  ==========================================================================================

namespace GameFrameX.OperationKeyboard.Runtime
{
    /// <summary>
    /// 键盘拉起参数。
    /// </summary>
    /// <remarks>
    /// 各小游戏平台 showKeyboard 的公共参数面；渠道包按平台语义直通或映射。
    /// </remarks>
    [UnityEngine.Scripting.Preserve]
    public sealed class OperationKeyboardShowOption
    {
        /// <summary>
        /// 初始化键盘拉起参数。
        /// </summary>
        public OperationKeyboardShowOption()
        {
        }

        /// <summary>
        /// 以初始值初始化键盘拉起参数。
        /// </summary>
        /// <param name="defaultValue">键盘输入框初始值。</param>
        public OperationKeyboardShowOption(string defaultValue)
        {
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// 获取或设置键盘输入框初始值（拉起时字段的当前文本）。
        /// </summary>
        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置最大输入长度，恒为 -1（不限长）。
        /// </summary>
        /// <remarks>
        /// 真机实证（unity-wx-adapt §⑤）：小游戏键盘的 maxLength 是键盘侧硬上限，
        /// 且中文输入法按拼音组合串长度计数——按 InputField.characterLimit 传硬上限时，
        /// 拼音先于汉字撞墙、组合被掐断，表现为“无法输入中文只能输英文”。
        /// 因此统一传 -1（不限长），长度约束由游戏侧在 input 回调中自行裁剪；
        /// “不限长”的平台方言（如支付宝以 0 表示）由渠道包内部映射，不对调用方暴露。
        /// Length cap is always -1: mini-game keyboards count the pinyin composition string,
        /// so a hard cap truncates Chinese input mid-composition. Platform dialects are mapped inside channel packages.
        /// </remarks>
        public int MaxLength { get; set; } = -1;

        /// <summary>
        /// 获取或设置是否多行输入。
        /// </summary>
        public bool Multiple { get; set; }

        /// <summary>
        /// 获取或设置点击确认按钮时键盘是否收起（true 不收起）。
        /// </summary>
        public bool ConfirmHold { get; set; }

        /// <summary>
        /// 获取或设置确认按钮类型（"done" / "next" / "search" / "go" / "send"）。
        /// </summary>
        public string ConfirmType { get; set; } = "done";
    }
}
