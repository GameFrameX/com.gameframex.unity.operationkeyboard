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
using NUnit.Framework;

namespace GameFrameX.OperationKeyboard.Tests
{
    using GameFrameX.OperationKeyboard.Runtime;
    // 门面类名与命名空间片段 GameFrameX.OperationKeyboard 同名：文件级 using 会被外层命名空间遮蔽，
    // 必须在命名空间体内用别名导入（C30 执行经验，notes.md「门面命名遮蔽」）。
    using KeyboardFacade = GameFrameX.OperationKeyboard.Runtime.OperationKeyboard;

    /// <summary>
    /// 键盘渠道注册表与字段会话单槽仲裁用例。
    /// </summary>
    /// <remarks>
    /// 覆盖：注册表语义（懒实例化与缓存 / 重复注册保首 / null 注册拒绝 / Reset / 互斥解析）、
    /// 单槽仲裁（新字段 Attach 顶替旧字段自动 Hide + EndEdit / 过期 Detach no-op）、
    /// 未注册渠道 no-op（门面不拦截、不触碰字段）、
    /// 事件流 → 原生事件映射（input → text + 置尾光标；confirm / complete → 终值 + Hide + EndEdit）。
    /// </remarks>
    public class OperationKeyboardRegistryTests
    {
        private class StubKeyboardManager : IOperationKeyboardManager
        {
            public int ShowCallCount;
            public int HideCallCount;
            public OperationKeyboardShowOption LastShowOption;
            public string LastUpdateValue;
            public Action<string> InputHandler;
            public Action<string> ConfirmHandler;
            public Action<string> CompleteHandler;
            public Action<float> HeightChangedHandler;

            public void Show(OperationKeyboardShowOption option)
            {
                ShowCallCount++;
                LastShowOption = option;
            }

            public void Hide()
            {
                HideCallCount++;
            }

            public void UpdateValue(string value)
            {
                LastUpdateValue = value;
            }

            public void OnInput(Action<string> handler)
            {
                InputHandler = handler;
            }

            public void OnConfirm(Action<string> handler)
            {
                ConfirmHandler = handler;
            }

            public void OnComplete(Action<string> handler)
            {
                CompleteHandler = handler;
            }

            public void OnHeightChanged(Action<float> handler)
            {
                HeightChangedHandler = handler;
            }

            public void RaiseInput(string value)
            {
                InputHandler?.Invoke(value);
            }

            public void RaiseConfirm(string value)
            {
                ConfirmHandler?.Invoke(value);
            }

            public void RaiseComplete(string value)
            {
                CompleteHandler?.Invoke(value);
            }

            public void RaiseHeightChanged(float height)
            {
                HeightChangedHandler?.Invoke(height);
            }
        }

        private sealed class OtherKeyboardManager : StubKeyboardManager
        {
        }

        private class StubKeyboardField : IOperationKeyboardField
        {
            public string Text { get; set; } = string.Empty;
            public bool IsFocused => true;
            public bool Multiline { get; set; }
            public string ConfirmType { get; set; } = "done";
            public int MoveTextEndCallCount;
            public int TriggerEndEditCallCount;

            public void MoveTextEnd()
            {
                MoveTextEndCallCount++;
            }

            public void TriggerEndEdit()
            {
                TriggerEndEditCallCount++;
            }
        }

        [SetUp]
        public void SetUp()
        {
            OperationKeyboardRegistry.Reset();
            KeyboardFacade.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            OperationKeyboardRegistry.Reset();
            KeyboardFacade.Reset();
        }

        [Test]
        public void Register_ThenGetManager_CreatesInstanceLazilyAndCaches()
        {
            var factoryCallCount = 0;
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () =>
            {
                factoryCallCount++;
                return new StubKeyboardManager();
            }));

            Assert.IsTrue(OperationKeyboardRegistry.Contains("stub"));
            Assert.AreEqual(0, factoryCallCount, "工厂在首次获取前不应被调用");

            var first = OperationKeyboardRegistry.GetManager("stub");
            var second = OperationKeyboardRegistry.GetManager("stub");

            Assert.IsNotNull(first);
            Assert.IsInstanceOf<StubKeyboardManager>(first);
            Assert.AreSame(first, second, "同一渠道应复用实例");
            Assert.AreEqual(1, factoryCallCount);
        }

        [Test]
        public void Register_DuplicateChannelId_KeepsFirstRegistration()
        {
            var first = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "第一个", () => first));
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "第二个", () => new OtherKeyboardManager()));

            Assert.AreSame(first, OperationKeyboardRegistry.GetManager("stub"), "重复注册应保留首个");
        }

        [Test]
        public void Register_NullArguments_AreIgnoredOrThrown()
        {
            OperationKeyboardRegistry.Register(null);
            Assert.IsFalse(OperationKeyboardRegistry.Contains("stub"));

            Assert.Throws<ArgumentNullException>(() => new OperationKeyboardChannelRegistration(null, "无效", () => new StubKeyboardManager()));
            Assert.Throws<ArgumentNullException>(() => new OperationKeyboardChannelRegistration("stub", "无效", null));
        }

        [Test]
        public void Unregister_RemovesRegistrationAndCachedManager()
        {
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => new StubKeyboardManager()));
            Assert.IsNotNull(OperationKeyboardRegistry.GetManager("stub"));

            OperationKeyboardRegistry.Unregister("stub");

            Assert.IsFalse(OperationKeyboardRegistry.Contains("stub"));
            Assert.IsNull(OperationKeyboardRegistry.GetManager("stub"));
        }

        [Test]
        public void Reset_AllowsReRegistrationAfterDomainReloadDisabled()
        {
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => new StubKeyboardManager()));

            OperationKeyboardRegistry.Reset();

            Assert.IsFalse(OperationKeyboardRegistry.Contains("stub"));
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => new OtherKeyboardManager()));
            Assert.IsInstanceOf<OtherKeyboardManager>(OperationKeyboardRegistry.GetManager("stub"));
        }

        [Test]
        public void ResolveManager_NoRegistration_ReturnsNull()
        {
            Assert.IsNull(OperationKeyboardRegistry.ResolveManager());
        }

        [Test]
        public void ResolveManager_MultipleRegistrations_ResolvesFirstRegistered()
        {
            var first = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("first", "第一个", () => first));
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("second", "第二个", () => new OtherKeyboardManager()));

            Assert.AreSame(first, OperationKeyboardRegistry.ResolveManager(), "互斥解析应按注册序取首个");
            Assert.IsInstanceOf<OtherKeyboardManager>(OperationKeyboardRegistry.GetManager("second"), "指定渠道仍可显式获取");
        }

        [Test]
        public void Attach_WithoutChannelRegistration_IsNoOpAndLeavesFieldUntouched()
        {
            var field = new StubKeyboardField { Text = "abc" };

            Assert.IsFalse(KeyboardFacade.IsChannelActive);
            KeyboardFacade.Attach(field);
            KeyboardFacade.Hide();
            KeyboardFacade.UpdateValue("xyz");

            Assert.IsNull(KeyboardFacade.CurrentField, "未注册渠道时不应建立字段会话");
            Assert.AreEqual("abc", field.Text, "未注册渠道时门面不得触碰字段");
            Assert.AreEqual(0, field.MoveTextEndCallCount);
            Assert.AreEqual(0, field.TriggerEndEditCallCount);
        }

        [Test]
        public void Attach_ShowsKeyboardWithFieldDerivedOption()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var field = new StubKeyboardField { Text = "初始文本", Multiline = true, ConfirmType = "go" };

            KeyboardFacade.Attach(field);

            Assert.AreSame(field, KeyboardFacade.CurrentField);
            Assert.AreEqual(1, manager.ShowCallCount);
            Assert.AreEqual("初始文本", manager.LastShowOption.DefaultValue);
            Assert.AreEqual(-1, manager.LastShowOption.MaxLength, "maxLength 必须恒为 -1（拼音组合计数实证）");
            Assert.IsTrue(manager.LastShowOption.Multiple);
            Assert.AreEqual("go", manager.LastShowOption.ConfirmType);
        }

        [Test]
        public void Attach_NewField_PreemptsPreviousFieldWithHideAndEndEdit()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var first = new StubKeyboardField { Text = "第一个" };
            var second = new StubKeyboardField { Text = "第二个" };

            KeyboardFacade.Attach(first);
            KeyboardFacade.Attach(second);

            Assert.AreEqual(1, manager.HideCallCount, "新字段 Attach 应自动收起键盘");
            Assert.AreEqual(1, first.TriggerEndEditCallCount, "旧字段应被触发一次 EndEdit（模拟原生失焦）");
            Assert.AreSame(second, KeyboardFacade.CurrentField, "新字段应持有会话");
            Assert.AreEqual(0, second.TriggerEndEditCallCount);
            Assert.AreEqual(2, manager.ShowCallCount, "两个字段各拉起一次键盘");
        }

        [Test]
        public void Attach_SameField_ReShowsWithoutPreemption()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var field = new StubKeyboardField { Text = "同一字段" };

            KeyboardFacade.Attach(field);
            KeyboardFacade.Attach(field);

            Assert.AreEqual(0, manager.HideCallCount, "同一字段重复 Attach 不应触发顶替");
            Assert.AreEqual(0, field.TriggerEndEditCallCount);
            Assert.AreEqual(2, manager.ShowCallCount, "重复 Attach 应重新拉起键盘（键盘被收起后的再点场景）");
        }

        [Test]
        public void InputEvent_AssignsTextAndMovesCursorToEnd()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var field = new StubKeyboardField();
            KeyboardFacade.Attach(field);

            manager.RaiseInput("中文输入");

            Assert.AreEqual("中文输入", field.Text, "input 事件应回灌字段文本（onValueChanged 由字段自身照常触发）");
            Assert.AreEqual(1, field.MoveTextEndCallCount, "赋值后光标应置尾");
            Assert.AreSame(field, KeyboardFacade.CurrentField, "input 不应结束会话");
        }

        [Test]
        public void ConfirmEvent_FinalizesValueHidesKeyboardAndTriggersEndEdit()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var field = new StubKeyboardField { Text = "中间态" };
            KeyboardFacade.Attach(field);

            manager.RaiseConfirm("终值");

            Assert.AreEqual("终值", field.Text, "confirm 应落定终值");
            Assert.AreEqual(1, field.TriggerEndEditCallCount, "confirm 应触发 EndEdit（与原生移动端一致）");
            Assert.IsNull(KeyboardFacade.CurrentField, "confirm 后会话应结束");
            Assert.GreaterOrEqual(manager.HideCallCount, 1, "confirm 应收起键盘");
        }

        [Test]
        public void CompleteEvent_FinalizesSessionLikeConfirm()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var field = new StubKeyboardField();
            KeyboardFacade.Attach(field);

            manager.RaiseComplete("完成值");

            Assert.AreEqual("完成值", field.Text);
            Assert.AreEqual(1, field.TriggerEndEditCallCount);
            Assert.IsNull(KeyboardFacade.CurrentField);
        }

        [Test]
        public void StrayEventAfterSessionEnded_DoesNotTouchFields()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var field = new StubKeyboardField { Text = "稳定值" };
            KeyboardFacade.Attach(field);
            manager.RaiseConfirm("终值");

            manager.RaiseInput("迟到的输入");
            manager.RaiseComplete("迟到的完成");

            Assert.AreEqual("终值", field.Text, "会话结束后的迟到事件不应改写字段");
            Assert.AreEqual(1, field.TriggerEndEditCallCount);
        }

        [Test]
        public void Detach_CurrentField_HidesKeyboardWithoutEndEdit()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var field = new StubKeyboardField();
            KeyboardFacade.Attach(field);

            KeyboardFacade.Detach(field);

            Assert.AreEqual(1, manager.HideCallCount);
            Assert.IsNull(KeyboardFacade.CurrentField);
            Assert.AreEqual(0, field.TriggerEndEditCallCount, "失焦链路的 Detach 不应重复触发 EndEdit（失焦事件由字段自身产生）");
        }

        [Test]
        public void Detach_StaleField_IsNoOp()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var stale = new StubKeyboardField();
            var current = new StubKeyboardField();
            KeyboardFacade.Attach(stale);
            KeyboardFacade.Attach(current);
            var hideCountAfterPreemption = manager.HideCallCount;

            KeyboardFacade.Detach(stale);

            Assert.AreEqual(hideCountAfterPreemption, manager.HideCallCount, "过期解绑应为 no-op");
            Assert.AreSame(current, KeyboardFacade.CurrentField, "过期解绑不应影响当前会话");
        }

        [Test]
        public void Detach_NullField_IsNoOp()
        {
            KeyboardFacade.Detach(null);
            Assert.IsNull(KeyboardFacade.CurrentField);
        }

        [Test]
        public void Hide_ClearsSessionWithoutEndEdit()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var field = new StubKeyboardField();
            KeyboardFacade.Attach(field);

            KeyboardFacade.Hide();

            Assert.AreEqual(1, manager.HideCallCount);
            Assert.IsNull(KeyboardFacade.CurrentField);
            Assert.AreEqual(0, field.TriggerEndEditCallCount);
        }

        [Test]
        public void UpdateValue_And_HeightChanged_ForwardToResolvedManager()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            float? observedHeight = null;

            KeyboardFacade.UpdateValue("程序赋值");
            KeyboardFacade.OnHeightChanged(height => observedHeight = height);
            manager.RaiseHeightChanged(216f);

            Assert.AreEqual("程序赋值", manager.LastUpdateValue);
            Assert.AreEqual(216f, observedHeight);
        }

        [Test]
        public void FacadeReset_ClearsCurrentField()
        {
            var manager = new StubKeyboardManager();
            OperationKeyboardRegistry.Register(new OperationKeyboardChannelRegistration("stub", "存根渠道", () => manager));
            var field = new StubKeyboardField();
            KeyboardFacade.Attach(field);

            KeyboardFacade.Reset();

            Assert.IsNull(KeyboardFacade.CurrentField);
        }
    }
}
