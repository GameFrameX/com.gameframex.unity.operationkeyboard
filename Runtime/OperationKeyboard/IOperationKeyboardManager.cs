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

using System;

namespace GameFrameX.OperationKeyboard.Runtime
{
    /// <summary>
    /// 键盘管理器契约。
    /// </summary>
    /// <remarks>
    /// 小游戏平台（微信 / 支付宝 / 快手 / 抖音）WebGL 下 Unity 原生键盘路径（TouchScreenKeyboard）不可达，
    /// 本契约由渠道包实现为平台键盘 API（wx / my / ks / tt 的 showKeyboard 家族）的直通驱动器。
    /// 事件为单槽回调（注册序覆盖），由 <see cref="OperationKeyboard"/> 门面在字段会话内统一登记，
    /// 调用方不应自行多次订阅；<see cref="Hide"/> 时实现必须注销全部平台事件（Off 全部），防止跨会话串扰。
    /// 部分平台缺少个别能力（如支付宝无 UpdateKeyboard / 键盘高度事件）时按契约空实现并在实现内注明，
    /// 核心契约不因单平台缺席而缩面。
    /// The manager contract drives the platform showKeyboard family; events are single-slot callbacks
    /// registered by the facade per field session, and Hide must unregister every platform event.
    /// </remarks>
    [UnityEngine.Scripting.Preserve]
    public interface IOperationKeyboardManager
    {
        /// <summary>
        /// 拉起键盘。
        /// </summary>
        /// <param name="option">键盘参数；<see cref="OperationKeyboardShowOption.MaxLength"/> 恒为 -1（不限长）。</param>
        void Show(OperationKeyboardShowOption option);

        /// <summary>
        /// 收起键盘并注销全部平台事件。
        /// </summary>
        void Hide();

        /// <summary>
        /// 更新键盘输入框内容（仅键盘处于拉起状态时有效；平台不支持时为空实现）。
        /// </summary>
        /// <param name="value">当前输入框内容。</param>
        void UpdateValue(string value);

        /// <summary>
        /// 登记键盘输入事件回调（单槽，后设覆盖先设）。
        /// </summary>
        /// <param name="handler">回调：键盘输入的当前值。</param>
        void OnInput(Action<string> handler);

        /// <summary>
        /// 登记键盘确认事件回调（单槽，后设覆盖先设）。
        /// </summary>
        /// <param name="handler">回调：确认时的当前值。</param>
        void OnConfirm(Action<string> handler);

        /// <summary>
        /// 登记键盘完成（收起）事件回调（单槽，后设覆盖先设）。
        /// </summary>
        /// <param name="handler">回调：完成时的终值。</param>
        void OnComplete(Action<string> handler);

        /// <summary>
        /// 登记键盘高度变化事件回调（供避让布局消费；平台不支持时为空实现）。
        /// </summary>
        /// <param name="handler">回调：键盘高度（像素，0 表示收起）。</param>
        void OnHeightChanged(Action<float> handler);
    }
}
