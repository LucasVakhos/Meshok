namespace MeshokBrowser;
partial class MainMeshok
{
    /// <summary>
    /// Обязательная переменная конструктора.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    /// <summary>
    /// Освободить все используемые ресурсы.
    /// </summary>
    /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            if (mainBrowser != null)
                mainBrowser.Dispose();
            mainBrowser = null;
            if (messageSettings != null)
                messageSettings.Dispose();
            messageSettings = null;
            components.Dispose();
        }
        base.Dispose(disposing);
    }
    /// <summary>
    /// Требуемый метод для поддержки конструктора — не изменяйте
    /// содержимое этого метода с помощью редактора кода.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainMeshok));
        barExit = new DevExpress.XtraBars.BarButtonItem();
        ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
        appMenu = new DevExpress.XtraBars.Ribbon.ApplicationMenu(components);
        skinGallery = new DevExpress.XtraBars.SkinRibbonGalleryBarItem();
        btnProcessOrders = new DevExpress.XtraBars.BarButtonItem();
        btnDeleteLots = new DevExpress.XtraBars.BarButtonItem();
        btnCreateLots = new DevExpress.XtraBars.BarButtonItem();
        btnLoadLots = new DevExpress.XtraBars.BarButtonItem();
        btnProgramSetting = new DevExpress.XtraBars.BarButtonItem();
        btnMessageSetting = new DevExpress.XtraBars.BarButtonItem();
        btnDeleteSold = new DevExpress.XtraBars.BarButtonItem();
        btnAddPrix = new DevExpress.XtraBars.BarButtonItem();
        pageProcess = new DevExpress.XtraBars.Ribbon.RibbonPage();
        orderGroup = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
        goodsGroupLight = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
        pageGlobalOp = new DevExpress.XtraBars.Ribbon.RibbonPage();
        goodsGroup = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
        pageSettigs = new DevExpress.XtraBars.Ribbon.RibbonPage();
        ribbonPageGroup4 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
        ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
        ribbonStatusBar1 = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
        popupMenu1 = new DevExpress.XtraBars.PopupMenu(components);
        dockManager = new DevExpress.XtraBars.Docking.DockManager(components);
        documentManager = new DevExpress.XtraBars.Docking2010.DocumentManager(components);
        tabbedView = new DevExpress.XtraBars.Docking2010.Views.Tabbed.TabbedView(components);
        noDocumentsView1 = new DevExpress.XtraBars.Docking2010.Views.NoDocuments.NoDocumentsView(components);
        aclMain = new ActionList();
        actProcessOrders = new ActionGh();
        actDeleteLots = new ActionGh();
        actCreateLots = new ActionGh();
        actLoadLots = new ActionGh();
        actProgramSetting = new ActionGh();
        actMessageSetting = new ActionGh();
        actRemoveRasx = new ActionGh();
        actAddPrix = new ActionGh();
        ribbonPageGroup2 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
        ((System.ComponentModel.ISupportInitialize)ribbonControl1).BeginInit();
        ((System.ComponentModel.ISupportInitialize)appMenu).BeginInit();
        ((System.ComponentModel.ISupportInitialize)popupMenu1).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dockManager).BeginInit();
        ((System.ComponentModel.ISupportInitialize)documentManager).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tabbedView).BeginInit();
        ((System.ComponentModel.ISupportInitialize)noDocumentsView1).BeginInit();
        ((System.ComponentModel.ISupportInitialize)aclMain).BeginInit();
        SuspendLayout();
        // 
        // barExit
        // 
        aclMain.SetAction(barExit, null);
        barExit.Caption = "Выход из програмы";
        barExit.CategoryGuid = new Guid("da008c6e-dd35-439e-b400-55f9f2b58ef7");
        barExit.Id = 2;
        barExit.ImageOptions.ImageUri.Uri = "Close;Size32x32";
        barExit.Name = "barExit";
        barExit.ItemClick += barExit_ItemClick;
        // 
        // ribbonControl1
        // 
        ribbonControl1.ApplicationButtonDropDownControl = appMenu;
        ribbonControl1.Categories.AddRange(new DevExpress.XtraBars.BarManagerCategory[] { new DevExpress.XtraBars.BarManagerCategory("menu", new Guid("b6ded52f-7f26-4ed3-b22e-05448cf20569")), new DevExpress.XtraBars.BarManagerCategory("status", new Guid("55328c65-9a01-4f8b-ae8c-e97b57ca5785")), new DevExpress.XtraBars.BarManagerCategory("m_file", new Guid("da008c6e-dd35-439e-b400-55f9f2b58ef7")), new DevExpress.XtraBars.BarManagerCategory("m_service", new Guid("858553f9-34ad-43d3-8eb9-e3d545b81905")) });
        ribbonControl1.ExpandCollapseItem.Id = 0;
        ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] { ribbonControl1.ExpandCollapseItem, barExit, skinGallery, btnProcessOrders, btnDeleteLots, btnCreateLots, btnLoadLots, btnProgramSetting, btnMessageSetting, btnDeleteSold, btnAddPrix });
        ribbonControl1.Location = new Point(0, 0);
        ribbonControl1.MaxItemId = 4;
        ribbonControl1.MdiMergeStyle = DevExpress.XtraBars.Ribbon.RibbonMdiMergeStyle.Always;
        ribbonControl1.Name = "ribbonControl1";
        ribbonControl1.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] { pageProcess, pageGlobalOp, pageSettigs });
        ribbonControl1.ShowToolbarCustomizeItem = false;
        ribbonControl1.Size = new Size(978, 126);
        ribbonControl1.StatusBar = ribbonStatusBar1;
        ribbonControl1.Toolbar.ShowCustomizeItem = false;
        ribbonControl1.ToolbarLocation = DevExpress.XtraBars.Ribbon.RibbonQuickAccessToolbarLocation.Hidden;
        // 
        // appMenu
        // 
        appMenu.ItemLinks.Add(barExit);
        appMenu.Name = "appMenu";
        appMenu.Ribbon = ribbonControl1;
        // 
        // skinGallery
        // 
        aclMain.SetAction(skinGallery, null);
        skinGallery.Caption = "Галерея";
        skinGallery.Id = 2;
        skinGallery.Name = "skinGallery";
        // 
        // btnProcessOrders
        // 
        aclMain.SetAction(btnProcessOrders, actProcessOrders);
        btnProcessOrders.Caption = "Обработка заказов";
        btnProcessOrders.Id = 2;
        btnProcessOrders.ImageOptions.Image = (Image)resources.GetObject("btnProcessOrders.ImageOptions.Image");
        btnProcessOrders.ImageOptions.LargeImage = (Image)resources.GetObject("btnProcessOrders.ImageOptions.LargeImage");
        btnProcessOrders.Name = "btnProcessOrders";
        // 
        // btnDeleteLots
        // 
        aclMain.SetAction(btnDeleteLots, actDeleteLots);
        btnDeleteLots.Caption = "Снять все лоты";
        btnDeleteLots.Hint = "Снять весь непроданный товар";
        btnDeleteLots.Id = 3;
        btnDeleteLots.ImageOptions.Image = (Image)resources.GetObject("btnDeleteLots.ImageOptions.Image");
        btnDeleteLots.ImageOptions.LargeImage = (Image)resources.GetObject("btnDeleteLots.ImageOptions.LargeImage");
        btnDeleteLots.Name = "btnDeleteLots";
        // 
        // btnCreateLots
        // 
        aclMain.SetAction(btnCreateLots, actCreateLots);
        btnCreateLots.Caption = "Создать CSV-файлы";
        btnCreateLots.Hint = "Создать CSV-файлы для импорта лотов";
        btnCreateLots.Id = 4;
        btnCreateLots.ImageOptions.Image = (Image)resources.GetObject("btnCreateLots.ImageOptions.Image");
        btnCreateLots.ImageOptions.LargeImage = (Image)resources.GetObject("btnCreateLots.ImageOptions.LargeImage");
        btnCreateLots.Name = "btnCreateLots";
        // 
        // btnLoadLots
        // 
        aclMain.SetAction(btnLoadLots, actLoadLots);
        btnLoadLots.Caption = "Загрузить лоты";
        btnLoadLots.Hint = "Загрузить лоты из CSV-файлов";
        btnLoadLots.Id = 5;
        btnLoadLots.ImageOptions.Image = (Image)resources.GetObject("btnLoadLots.ImageOptions.Image");
        btnLoadLots.ImageOptions.LargeImage = (Image)resources.GetObject("btnLoadLots.ImageOptions.LargeImage");
        btnLoadLots.Name = "btnLoadLots";
        // 
        // btnProgramSetting
        // 
        aclMain.SetAction(btnProgramSetting, actProgramSetting);
        btnProgramSetting.Caption = "Настройки среды";
        btnProgramSetting.Hint = "Настройки подключения и прочие установки";
        btnProgramSetting.Id = 6;
        btnProgramSetting.ImageOptions.Image = (Image)resources.GetObject("btnProgramSetting.ImageOptions.Image");
        btnProgramSetting.ImageOptions.LargeImage = (Image)resources.GetObject("btnProgramSetting.ImageOptions.LargeImage");
        btnProgramSetting.Name = "btnProgramSetting";
        // 
        // btnMessageSetting
        // 
        aclMain.SetAction(btnMessageSetting, actMessageSetting);
        btnMessageSetting.Caption = "Настройки сообшений";
        btnMessageSetting.Hint = "Настройки сообшений для личного кабинета покупателей";
        btnMessageSetting.Id = 7;
        btnMessageSetting.ImageOptions.Image = (Image)resources.GetObject("btnMessageSetting.ImageOptions.Image");
        btnMessageSetting.ImageOptions.LargeImage = (Image)resources.GetObject("btnMessageSetting.ImageOptions.LargeImage");
        btnMessageSetting.Name = "btnMessageSetting";
        // 
        // btnDeleteSold
        // 
        aclMain.SetAction(btnDeleteSold, actRemoveRasx);
        btnDeleteSold.Caption = "Снять проданные";
        btnDeleteSold.Hint = "Снять лоты проданные в базе";
        btnDeleteSold.Id = 2;
        btnDeleteSold.ImageOptions.Image = (Image)resources.GetObject("btnDeleteSold.ImageOptions.Image");
        btnDeleteSold.ImageOptions.LargeImage = (Image)resources.GetObject("btnDeleteSold.ImageOptions.LargeImage");
        btnDeleteSold.Name = "btnDeleteSold";
        // 
        // btnAddPrix
        // 
        aclMain.SetAction(btnAddPrix, actAddPrix);
        btnAddPrix.Caption = "Выставить приходы";
        btnAddPrix.Hint = "Выставить поступивший товар";
        btnAddPrix.Id = 3;
        btnAddPrix.ImageOptions.Image = (Image)resources.GetObject("btnAddPrix.ImageOptions.Image");
        btnAddPrix.ImageOptions.LargeImage = (Image)resources.GetObject("btnAddPrix.ImageOptions.LargeImage");
        btnAddPrix.Name = "btnAddPrix";
        // 
        // pageProcess
        // 
        pageProcess.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] { orderGroup, goodsGroupLight });
        pageProcess.Name = "pageProcess";
        pageProcess.Text = "ОПЕРАЦИИ";
        // 
        // orderGroup
        // 
        orderGroup.ItemLinks.Add(btnProcessOrders);
        orderGroup.Name = "orderGroup";
        orderGroup.Text = "Заказы";
        // 
        // goodsGroupLight
        // 
        goodsGroupLight.ItemLinks.Add(btnDeleteSold);
        goodsGroupLight.ItemLinks.Add(btnAddPrix);
        goodsGroupLight.Name = "goodsGroupLight";
        goodsGroupLight.Text = "Остатки";
        // 
        // pageGlobalOp
        // 
        pageGlobalOp.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] { goodsGroup });
        pageGlobalOp.Name = "pageGlobalOp";
        pageGlobalOp.Text = "ГЛОБАЛЬНЫЕ ОПЕРАЦИИ";
        // 
        // goodsGroup
        // 
        goodsGroup.ItemLinks.Add(btnDeleteLots);
        goodsGroup.ItemLinks.Add(btnCreateLots);
        goodsGroup.ItemLinks.Add(btnLoadLots);
        goodsGroup.Name = "goodsGroup";
        goodsGroup.Text = "Работа с товарными остатками";
        // 
        // pageSettigs
        // 
        pageSettigs.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] { ribbonPageGroup4, ribbonPageGroup1 });
        pageSettigs.Name = "pageSettigs";
        pageSettigs.Text = "НАСТРОЙКИ";
        // 
        // ribbonPageGroup4
        // 
        ribbonPageGroup4.ItemLinks.Add(btnProgramSetting);
        ribbonPageGroup4.ItemLinks.Add(btnMessageSetting);
        ribbonPageGroup4.Name = "ribbonPageGroup4";
        ribbonPageGroup4.Text = "Настройки програмы";
        // 
        // ribbonPageGroup1
        // 
        ribbonPageGroup1.ItemLinks.Add(skinGallery);
        ribbonPageGroup1.Name = "ribbonPageGroup1";
        ribbonPageGroup1.Text = "Внешний вид";
        // 
        // ribbonStatusBar1
        // 
        ribbonStatusBar1.Location = new Point(0, 535);
        ribbonStatusBar1.Name = "ribbonStatusBar1";
        ribbonStatusBar1.Ribbon = ribbonControl1;
        ribbonStatusBar1.Size = new Size(978, 27);
        // 
        // popupMenu1
        // 
        popupMenu1.Name = "popupMenu1";
        popupMenu1.Ribbon = ribbonControl1;
        // 
        // dockManager
        // 
        dockManager.Form = this;
        dockManager.TopZIndexControls.AddRange(new string[] { "DevExpress.XtraBars.BarDockControl", "DevExpress.XtraBars.StandaloneBarDockControl", "System.Windows.Forms.StatusBar", "System.Windows.Forms.MenuStrip", "System.Windows.Forms.StatusStrip", "DevExpress.XtraBars.Ribbon.RibbonStatusBar", "DevExpress.XtraBars.Ribbon.RibbonControl", "DevExpress.XtraBars.Navigation.OfficeNavigationBar", "DevExpress.XtraBars.Navigation.TileNavPane" });
        // 
        // documentManager
        // 
        documentManager.ContainerControl = this;
        documentManager.RibbonAndBarsMergeStyle = DevExpress.XtraBars.Docking2010.Views.RibbonAndBarsMergeStyle.Always;
        documentManager.View = tabbedView;
        documentManager.ViewCollection.AddRange(new DevExpress.XtraBars.Docking2010.Views.BaseView[] { tabbedView, noDocumentsView1 });
        // 
        // aclMain
        // 
        aclMain.Actions.Add(actProcessOrders);
        aclMain.Actions.Add(actDeleteLots);
        aclMain.Actions.Add(actCreateLots);
        aclMain.Actions.Add(actLoadLots);
        aclMain.Actions.Add(actProgramSetting);
        aclMain.Actions.Add(actMessageSetting);
        aclMain.Actions.Add(actRemoveRasx);
        aclMain.Actions.Add(actAddPrix);
        aclMain.Owner = this;
        // 
        // actProcessOrders
        // 
        actProcessOrders.Caption = "Обработка заказов";
        actProcessOrders.Image = (Image)resources.GetObject("actProcessOrders.Image");
        actProcessOrders.LargeImage = (Image)resources.GetObject("actProcessOrders.LargeImage");
        actProcessOrders.Tag = null;
        actProcessOrders.ToolTipText = "Сканирование раздела (Сделки с покупателями)";
        // 
        // actDeleteLots
        // 
        actDeleteLots.Caption = "Снять все лоты";
        actDeleteLots.Image = (Image)resources.GetObject("actDeleteLots.Image");
        actDeleteLots.LargeImage = (Image)resources.GetObject("actDeleteLots.LargeImage");
        actDeleteLots.Tag = null;
        actDeleteLots.ToolTipText = "Снять весь непроданный товар";
        // 
        // actCreateLots
        // 
        actCreateLots.Caption = "Создать CSV-файлы";
        actCreateLots.Image = (Image)resources.GetObject("actCreateLots.Image");
        actCreateLots.LargeImage = (Image)resources.GetObject("actCreateLots.LargeImage");
        actCreateLots.Tag = null;
        actCreateLots.ToolTipText = "Создать CSV-файлы для импорта лотов";
        // 
        // actLoadLots
        // 
        actLoadLots.Caption = "Загрузить лоты";
        actLoadLots.Image = (Image)resources.GetObject("actLoadLots.Image");
        actLoadLots.LargeImage = (Image)resources.GetObject("actLoadLots.LargeImage");
        actLoadLots.Tag = null;
        actLoadLots.ToolTipText = "Загрузить лоты из CSV-файлов";
        // 
        // actProgramSetting
        // 
        actProgramSetting.Caption = "Настройки среды";
        actProgramSetting.Image = (Image)resources.GetObject("actProgramSetting.Image");
        actProgramSetting.LargeImage = (Image)resources.GetObject("actProgramSetting.LargeImage");
        actProgramSetting.Tag = null;
        actProgramSetting.ToolTipText = "Настройки подключения и прочие установки";
        // 
        // actMessageSetting
        // 
        actMessageSetting.Caption = "Настройки сообшений";
        actMessageSetting.Image = (Image)resources.GetObject("actMessageSetting.Image");
        actMessageSetting.LargeImage = (Image)resources.GetObject("actMessageSetting.LargeImage");
        actMessageSetting.Tag = null;
        actMessageSetting.ToolTipText = "Настройки сообшений для личного кабинета покупателей";
        // 
        // actRemoveRasx
        // 
        actRemoveRasx.Caption = "Снять проданный товар";
        actRemoveRasx.Image = (Image)resources.GetObject("actRemoveRasx.Image");
        actRemoveRasx.LargeImage = (Image)resources.GetObject("actRemoveRasx.LargeImage");
        actRemoveRasx.Tag = null;
        actRemoveRasx.ToolTipText = "Снять лоты проданные в базе";
        // 
        // actAddPrix
        // 
        actAddPrix.Caption = "Выставить приходы";
        actAddPrix.Image = (Image)resources.GetObject("actAddPrix.Image");
        actAddPrix.LargeImage = (Image)resources.GetObject("actAddPrix.LargeImage");
        actAddPrix.Tag = null;
        actAddPrix.ToolTipText = "Выставить поступивший товар";
        actAddPrix.Update += ActAddPrix_Update;
        // 
        // ribbonPageGroup2
        // 
        ribbonPageGroup2.Name = "ribbonPageGroup2";
        ribbonPageGroup2.Text = "ribbonPageGroup2";
        // 
        // MainMeshok
        // 
        AutoScaleDimensions = new SizeF(6F, 13F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(978, 562);
        Controls.Add(ribbonControl1);
        Controls.Add(ribbonStatusBar1);
        IconOptions.Icon = (Icon)resources.GetObject("MainMeshok.IconOptions.Icon");
        Name = "MainMeshok";
        StartPosition = FormStartPosition.Manual;
        Text = "Мой Мешок";
        FormClosing += MainMeshok_FormClosing;
        ((System.ComponentModel.ISupportInitialize)ribbonControl1).EndInit();
        ((System.ComponentModel.ISupportInitialize)appMenu).EndInit();
        ((System.ComponentModel.ISupportInitialize)popupMenu1).EndInit();
        ((System.ComponentModel.ISupportInitialize)dockManager).EndInit();
        ((System.ComponentModel.ISupportInitialize)documentManager).EndInit();
        ((System.ComponentModel.ISupportInitialize)tabbedView).EndInit();
        ((System.ComponentModel.ISupportInitialize)noDocumentsView1).EndInit();
        ((System.ComponentModel.ISupportInitialize)aclMain).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
    private DevExpress.XtraBars.BarButtonItem barExit;
    private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
    private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar1;
    private DevExpress.XtraBars.Ribbon.ApplicationMenu appMenu;
    private DevExpress.XtraBars.SkinRibbonGalleryBarItem skinGallery;
    private DevExpress.XtraBars.Ribbon.RibbonPage pageProcess;
    private DevExpress.XtraBars.Ribbon.RibbonPage pageSettigs;
    private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup4;
    private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
    private DevExpress.XtraBars.PopupMenu popupMenu1;
    private DevExpress.XtraBars.Docking.DockManager dockManager;
    private DevExpress.XtraBars.Docking2010.DocumentManager documentManager;
    private DevExpress.XtraBars.Docking2010.Views.NoDocuments.NoDocumentsView noDocumentsView1;
    private DevExpress.XtraBars.Docking2010.Views.Tabbed.TabbedView tabbedView;
    private LB.Libs.ActionList aclMain;
    private LB.Libs.ActionGh actProcessOrders;
    private LB.Libs.ActionGh actDeleteLots;
    private LB.Libs.ActionGh actCreateLots;
    private LB.Libs.ActionGh actLoadLots;
    private LB.Libs.ActionGh actProgramSetting;
    private LB.Libs.ActionGh actMessageSetting;
    private DevExpress.XtraBars.BarButtonItem btnProcessOrders;
    private DevExpress.XtraBars.Ribbon.RibbonPageGroup orderGroup;
    private DevExpress.XtraBars.BarButtonItem btnDeleteLots;
    private DevExpress.XtraBars.BarButtonItem btnCreateLots;
    private DevExpress.XtraBars.BarButtonItem btnLoadLots;
    private DevExpress.XtraBars.Ribbon.RibbonPageGroup goodsGroup;
    private DevExpress.XtraBars.BarButtonItem btnProgramSetting;
    private DevExpress.XtraBars.BarButtonItem btnMessageSetting;
    private LB.Libs.ActionGh actRemoveRasx;
    private DevExpress.XtraBars.BarButtonItem btnDeleteSold;
    private DevExpress.XtraBars.Ribbon.RibbonPageGroup goodsGroupLight;
    private LB.Libs.ActionGh actAddPrix;
    private DevExpress.XtraBars.BarButtonItem btnAddPrix;
    private DevExpress.XtraBars.Ribbon.RibbonPage pageGlobalOp;
    private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup2;
}
