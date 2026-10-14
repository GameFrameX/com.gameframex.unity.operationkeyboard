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
    /// UI 无关的键盘字段契约。
    /// </summary>
    /// <remarks>
    /// 由 UI 绑定包按工具集实现（UGUI InputField / TMP_InputField / FairyGUI GTextInput 等），
    /// 核心门面据此驱动字段：input 事件赋值 <see cref="Text"/> 并置尾光标，confirm / complete 事件终值落定后触发
    /// <see cref="TriggerEndEdit"/>（与原生移动端 onEndEdit 事件流一致，业务回调零改动）。
    /// Implemented by UI binding packages per toolkit; the facade drives the field through this contract.
    /// </remarks>
    [UnityEngine.Scripting.Preserve]
    public interface IOperationKeyboardField
    {
        /// <summary>
        /// 获取或设置字段当前文本。
        /// </summary>
        /// <remarks>
        /// 键盘 input 事件回灌时经此属性赋值（onValueChanged 等字段自身事件照常触发）。
        /// </remarks>
        string Text { get; set; }

        /// <summary>
        /// 获取字段当前是否聚焦。
        /// </summary>
        bool IsFocused { get; }

        /// <summary>
        /// 获取是否多行输入。
        /// </summary>
        bool Multiline { get; }

        /// <summary>
        /// 获取键盘确认按钮类型（"done" / "next" / "search" / "go" / "send"）。
        /// </summary>
        string ConfirmType { get; }

        /// <summary>
        /// 将光标移动到文本末尾。
        /// </summary>
        /// <remarks>
        /// 平台键盘 input 事件携带的是整串值，赋值后光标须置尾，避免光标停留在起始位置。
        /// </remarks>
        void MoveTextEnd();

        /// <summary>
        /// 触发字段的结束编辑事件（等价于原生失焦 / 键盘确认后的 onEndEdit）。
        /// </summary>
        void TriggerEndEdit();
    }
}
