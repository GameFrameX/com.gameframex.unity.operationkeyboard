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
    /// 键盘统一静态门面（含字段会话单槽仲裁）。
    /// </summary>
    /// <remarks>
    /// 键盘是独占资源，在注册表互斥解析之上增加字段级单槽仲裁：同一时刻仅一个 <see cref="IOperationKeyboardField"/>
    /// 持有键盘，新字段 <see cref="Attach"/> 时自动收起键盘并解绑旧字段（旧字段触发一次 EndEdit，模拟原生失焦事件流），
    /// 从根上消除“每字段各自订阅全局键盘事件、双字段并发即串扰”的适配器缺陷。
    /// 事件流映射（与原生移动端一致，业务回调零改动）：
    /// input → 字段 Text 赋值（onValueChanged 照常触发）+ 光标置尾；
    /// confirm / complete → 终值落定 + 收起键盘 + 触发字段 EndEdit；
    /// 失焦 → <see cref="Detach"/>（收起键盘，不重复触发 EndEdit，失焦事件由字段自身失焦链路产生）。
    /// 未注册渠道（Editor / 原生平台 / 未装小游戏 SDK）时本门面全部 no-op：
    /// 不拦截任何事件、不触碰字段，原生 TouchScreenKeyboard 路径完整保留。
    /// The facade owns single-slot field arbitration on top of registry-level mutual exclusion:
    /// attaching a new field hides the keyboard and ends the previous field's session.
    /// With no channel registered, every call is a no-op and the native keyboard path is preserved.
    /// </remarks>
    [UnityEngine.Scripting.Preserve]
    public static class OperationKeyboard
    {
        private static IOperationKeyboardField s_CurrentField;

        /// <summary>
        /// 获取当前持有键盘会话的字段（无会话时为 null）。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static IOperationKeyboardField CurrentField
        {
            [UnityEngine.Scripting.Preserve]
            get => s_CurrentField;
        }

        /// <summary>
        /// 获取当前运行环境是否已注册键盘渠道（false 时本门面为 no-op，原生键盘路径生效）。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static bool IsChannelActive
        {
            [UnityEngine.Scripting.Preserve]
            get => OperationKeyboardRegistry.ResolveManager() != null;
        }

        /// <summary>
        /// 重置门面会话状态（Domain Reload 关闭时避免残留旧字段引用；不触碰 JS 桥）。
        /// </summary>
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        [UnityEngine.Scripting.Preserve]
        public static void Reset()
        {
            s_CurrentField = null;
        }

        /// <summary>
        /// 绑定字段并拉起键盘（字段会话入口）。
        /// </summary>
        /// <remarks>
        /// 键盘参数由字段契约推导：DefaultValue 取字段当前文本、MaxLength 恒 -1、Multiple 取多行、ConfirmType 取字段确认类型。
        /// 同一字段重复 Attach 时仅重新拉起键盘；不同字段 Attach 时先收起键盘并对旧字段触发 EndEdit（单槽仲裁）。
        /// 未注册渠道时 no-op，不触碰字段。
        /// </remarks>
        /// <param name="field">要绑定键盘的字段。</param>
        [UnityEngine.Scripting.Preserve]
        public static void Attach(IOperationKeyboardField field)
        {
            var manager = OperationKeyboardRegistry.ResolveManager();
            if (manager == null || field == null)
            {
                return;
            }

            if (!ReferenceEquals(s_CurrentField, field))
            {
                var previous = s_CurrentField;
                s_CurrentField = null;
                if (previous != null)
                {
                    manager.Hide();
                    previous.TriggerEndEdit();
                }

                s_CurrentField = field;
                manager.OnInput(HandleInput);
                manager.OnConfirm(HandleFinalize);
                manager.OnComplete(HandleFinalize);
            }

            manager.Show(new OperationKeyboardShowOption(field.Text)
            {
                Multiple = field.Multiline,
                ConfirmType = field.ConfirmType,
            });
        }

        /// <summary>
        /// 解绑字段并收起键盘（字段失焦链路调用；不触发 EndEdit，失焦事件由字段自身产生）。
        /// </summary>
        /// <param name="field">要解绑的字段；与当前会话字段不一致时本调用为 no-op（过期解绑）。</param>
        [UnityEngine.Scripting.Preserve]
        public static void Detach(IOperationKeyboardField field)
        {
            if (field == null || !ReferenceEquals(s_CurrentField, field))
            {
                return;
            }

            s_CurrentField = null;
            OperationKeyboardRegistry.ResolveManager()?.Hide();
        }

        /// <summary>
        /// 收起键盘并结束当前字段会话（不触发 EndEdit；游戏侧主动收起场景使用）。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static void Hide()
        {
            s_CurrentField = null;
            OperationKeyboardRegistry.ResolveManager()?.Hide();
        }

        /// <summary>
        /// 更新键盘输入框内容（仅键盘拉起时有效）。
        /// </summary>
        /// <param name="value">当前输入框内容。</param>
        [UnityEngine.Scripting.Preserve]
        public static void UpdateValue(string value)
        {
            OperationKeyboardRegistry.ResolveManager()?.UpdateValue(value);
        }

        /// <summary>
        /// 登记键盘高度变化回调（供避让布局消费；平台不支持时为空实现）。
        /// </summary>
        /// <param name="handler">回调：键盘高度（像素，0 表示收起）。</param>
        [UnityEngine.Scripting.Preserve]
        public static void OnHeightChanged(Action<float> handler)
        {
            OperationKeyboardRegistry.ResolveManager()?.OnHeightChanged(handler);
        }

        private static void HandleInput(string value)
        {
            var field = s_CurrentField;
            if (field == null)
            {
                return;
            }

            field.Text = value ?? string.Empty;
            field.MoveTextEnd();
        }

        private static void HandleFinalize(string value)
        {
            var field = s_CurrentField;
            if (field == null)
            {
                return;
            }

            s_CurrentField = null;
            OperationKeyboardRegistry.ResolveManager()?.Hide();
            if (value != null)
            {
                field.Text = value;
            }

            field.TriggerEndEdit();
        }
    }
}
