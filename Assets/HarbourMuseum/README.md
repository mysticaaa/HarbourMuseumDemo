# Harbour City Museum — Unity 场景与资产原型

工程：`D:\workplace\Unity Projects\demo - 副本`

Unity 版本：**2022.3.62f3**，Built-in 渲染管线。

## 打开与运行

1. 用 Unity Hub 打开上述工程。
2. 在 Project 面板打开 `Assets/HarbourMuseum/Scenes/HarbourCityMuseum.unity`。
3. 点击 **Play**。建议 Game 视图使用 16:9、1600 × 900 或更高分辨率。
4. 在左上方导航中选择 **SHIP / STORIES / HELMET**，也可点击场景里的展品。入口会播放简短英文介绍。

如果使用已生成的桌面版本，运行 `Builds/HarbourMuseum/HarbourMuseum.exe`。保留同目录的数据文件和 DLL，不能只移动 EXE。

随附 `HarbourMuseum-Windows.zip` 是同一桌面版本的完整压缩包。解压后进入 `HarbourMuseum` 文件夹，运行 `HarbourMuseum.exe`；退出使用 Alt + F4。

随附 `HarbourMuseum-Assets.zip` 包含完整 `Assets/HarbourMuseum` 文件夹及原有 `.meta` 引用。要在另一 Unity 2022.3 工程复用，先解压，再把其中的 `Assets/HarbourMuseum` 文件夹复制到目标工程的 `Assets` 中，等待 Unity 导入，打开博物馆场景即可。若目标工程已有同名文件夹，先备份该文件夹。

## 操作

| 操作 | 作用 |
| --- | --- |
| 鼠标左键 | 射线选择展品、故事板或船舱兴趣点；点击屏幕大按钮 |
| 鼠标右键拖动 | 环顾四周 |
| WASD | 在展厅或船舱内移动 |
| 1 / 2 / 3 | 快速到达船模 / 故事墙 / 潜水头盔 |
| 展品界面旋转按钮 | 观察船模和头盔的不同方向 |
| Enter cabin | 进入独立的船舱观察区 |
| Return to gallery | 离开船舱 |
| 头盔 + / - 按钮 | 放大 / 缩小头盔 |
| Reset | 恢复展品初始旋转与尺寸 |

三站的完成状态显示在界面。五分钟计时用于引导体验，超时后可继续自由观察。

## 已搭建内容

| 需求 | 实现 |
| --- | --- |
| 三站博物馆体验 | 深蓝绿展厅、木地板、暖光、黄铜细节、接待台、座椅、展台与标识 |
| 船模 360° 观察 | 原创三桅帆船：曲面船壳、弧形帆、桅杆、绳索、甲板、货物与细节；旋转和复位 |
| 船舱观察 | 独立可环顾的重建船舱，包含座位、导航仪表、舵轮、货箱和工作绞盘；三个兴趣点可选择 |
| 港口故事 | 可选择的三段主题故事：海港劳动、迁移、日常生活；连续故事墙与阅读进度 |
| 古董潜水头盔 | 原创铜质圆顶、颈环、肩座、玻璃观察窗、格栅、螺栓和供气接口；旋转、缩放、复位 |
| 低门槛交互 | 点击射线、大按钮、快捷站点导航、高对比界面，避免依赖抓取手势 |
| 简短引导 | 本地生成的英文开场语音和屏幕说明 |
| 证据标注 | 场景与界面标明 interpretive reconstruction；不虚构特定城市、船只、年份或个人故事 |

## 资产与可编辑源文件

所有新增内容位于 `Assets/HarbourMuseum/`，原默认场景保留。

- `Scenes/HarbourCityMuseum.unity`：完整保存的场景，可直接在 Hierarchy 中编辑。
- `Prefabs/ThreeMastShip.prefab`：船模。
- `Prefabs/CopperDivingHelmet.prefab`：潜水头盔。
- `Prefabs/ShipCabin.prefab`：船舱。
- `Meshes/`：自制程序网格，持久化保存为 Unity 资产。
- `Materials/`：自制颜色及金属、木材、帆布、玻璃等材质。
- `Models/`：OBJ 与 MTL 交换文件，可导入 Maya / Blender。Unity Prefab 是主编辑资产；OBJ 不包含 Unity 脚本、字体和灯光。
- `Scripts/`：运行时交互逻辑。
- `Editor/`：场景构建器、原创模型生成器、OBJ 导出器与交互检查。
- `Audio/Welcome.wav`：英文引导语音。

可通过 Unity 菜单 **Harbour Museum → Build museum scene** 重建场景。此操作会重生成该博物馆场景及同名生成资产；已有手工改动应先另存副本。

可通过 **Harbour Museum → Run interaction smoke test** 检查交互，通过 **Build Windows prototype** 重新生成桌面版本。

**Export asset package** 菜单可在编辑器中另外导出 `.unitypackage`。本次随附的资产包格式为 ZIP。

## 内容依据与重建范围

此原型是一个虚构港城的解释性教学展厅，并非真实馆藏扫描或某艘船的精确复原。船舱布局、仪表组合、绞盘位置和头盔细节是为教学设计的重建。故事墙按主题与参观顺序组织，不声称是真实城市的年代史。

参考资料用于核对通用背景，没有下载或复用来源中的模型和图片：

- [London Museum：港口劳动](https://www.londonmuseum.org.uk/collections/london-stories/jobs-that-made-londons-docks-run/) — 货物搬运、仓储、记录及港口分工。
- [Museums Victoria：Journeys of a Lifetime](https://collections.museumsvictoria.com.au/articles/10647) — 海上迁移、行李及旅途日常生活。
- [Science Museum Group：Diving/Smoke helmet](https://collection.sciencemuseumgroup.org.uk/objects/co8004872/diving-smoke-helmet) — 铜质头盔与水面供气的背景。未采用该网页正文中有疑义的年份。
- [Royal Museums Greenwich：Cutty Sark](https://www.rmg.co.uk/cutty-sark/top-things-to-do) — 航运货物与船员劳动的参考案例；场景船只不被称为 Cutty Sark。

## XR 状态

当前可直接运行的是桌面交互原型。工程没有配置 SteamVR / OpenXR 的显示加载器，也没有进行 Meta Quest 3 真机测试。交互逻辑提供公共方法，方便以后由 XR 射线组件调用；目前使用键盘和鼠标输入。本次主要完成场景、自制资产和简单交互，头显部署仍需另行接入和验证。

## 验证记录

验证结果见随附的 `验证结果.txt`。预览图片由 Unity 实际渲染生成。
