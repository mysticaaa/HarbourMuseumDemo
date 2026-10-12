using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace HarbourMuseum.EditorTools
{
    public static class MuseumSceneBuilder
    {
        public const string Root = "Assets/HarbourMuseum";
        public const string ScenePath = Root + "/Scenes/HarbourCityMuseum.unity";
        static Material navy, wall, oak, brass, stone, teal, white, glow, glass, dark;
        static Font font;
        static Material textMaterial;

        [MenuItem("Harbour Museum/Build museum scene")]
        public static void Build()
        {
            foreach (string dir in new[] { "Scenes", "Prefabs", "Materials", "Meshes", "Audio" })
                Directory.CreateDirectory(Root + "/" + dir);
            AssetDatabase.Refresh();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            string textPath=Root+"/Materials/Gallery_DepthText.mat";
            textMaterial=AssetDatabase.LoadAssetAtPath<Material>(textPath);
            if(textMaterial==null){textMaterial=new Material(Shader.Find("HarbourMuseum/DepthText"));AssetDatabase.CreateAsset(textMaterial,textPath);}
            navy = Material("Gallery_Navy", new Color(.035f,.105f,.15f), .28f);
            wall = Material("Gallery_Petrol", new Color(.075f,.185f,.215f), .15f);
            oak = Material("Gallery_Oak", new Color(.43f,.245f,.105f), .22f);
            brass = Material("Gallery_Brass", new Color(.76f,.51f,.21f), .5f, .72f);
            stone = Material("Gallery_Limestone", new Color(.67f,.64f,.56f), .12f);
            teal = Material("Gallery_Teal", new Color(.1f,.41f,.41f), .28f);
            white = Material("Gallery_Ivory", new Color(.89f,.86f,.72f), .18f);
            dark = Material("Gallery_Iron", new Color(.045f,.055f,.065f), .32f, .5f);
            glow = Material("Gallery_Light", new Color(1f,.8f,.45f), .1f);
            glow.EnableKeyword("_EMISSION"); glow.SetColor("_EmissionColor", new Color(1f,.69f,.32f)*1.7f);
            glass = Material("Gallery_Window", new Color(.23f,.52f,.61f), .82f, .3f);
            var gallery = new GameObject("GALLERY | Harbour City Museum").transform;
            BuildGallery(gallery);
            var ship = MuseumModels.CreateShip(gallery);
            ship.name = "EXHIBIT 01 | Original three-mast ship";
            ship.transform.localPosition = new Vector3(-1f,.98f,0);
            ship.transform.localRotation = Quaternion.Euler(0,-12,0);
            AddTarget(ship, MuseumAction.Ship, "The ship / a port city's working tool", "An original interpretive model. Inspect the hull and rigging, then enter a reconstructed cabin.", new Vector3(0,1.75f,0), new Vector3(5.8f,3.8f,1.9f));
            PrefabUtility.SaveAsPrefabAsset(ship, Root + "/Prefabs/ThreeMastShip.prefab");
            var helmet = MuseumModels.CreateHelmet(gallery);
            helmet.name = "EXHIBIT 03 | Original copper diving helmet";
            helmet.transform.localPosition = new Vector3(5.15f,1.45f,.65f);
            AddTarget(helmet, MuseumAction.Helmet, "The diving helmet / work below water", "Surface-supplied air and sealed viewing ports supported underwater work. This is a reconstruction, not a collection scan.", new Vector3(0,.55f,0), new Vector3(1.4f,1.25f,1.4f));
            PrefabUtility.SaveAsPrefabAsset(helmet, Root + "/Prefabs/CopperDivingHelmet.prefab");
            var cabin = new GameObject("CABIN | Interpretive interior panorama").transform;
            cabin.position = new Vector3(0,0,40);
            BuildCabin(cabin);
            PrefabUtility.SaveAsPrefabAsset(cabin.gameObject, Root + "/Prefabs/ShipCabin.prefab");
            var cameraObject = new GameObject("Visitor camera"); cameraObject.tag="MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.fieldOfView = 57; camera.nearClipPlane=.08f; camera.farClipPlane=90;
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.025f,.05f,.075f);
            camera.allowHDR=true; camera.allowMSAA=true;
            var views = new GameObject("Accessible station viewpoints").transform;
            var galleryView=View("Welcome",views,new Vector3(6.8f,3.15f,-7.3f),new Vector3(-.6f,2.05f,.6f));
            var shipView=View("Ship",views,new Vector3(4.8f,2.75f,-4.8f),new Vector3(-1,2.4f,0));
            var helmetView=View("Helmet",views,new Vector3(5.15f,2.05f,-2.9f),new Vector3(5.15f,2.02f,.65f));
            var timelineView=View("Stories",views,new Vector3(0,2.45f,3.1f),new Vector3(0,2.55f,7.8f));
            var cabinView=View("Cabin",views,new Vector3(0,1.8f,38),new Vector3(0,1.6f,43));
            var overview=View("Overview",views,new Vector3(10,7.6f,-12),new Vector3(0,1.6f,1));
            camera.transform.SetPositionAndRotation(galleryView.position,galleryView.rotation);
            var experience=new GameObject("Museum experience | ray selection + accessible controls").AddComponent<MuseumExperience>();
            experience.visitorCamera=camera; experience.ship=ship.transform; experience.helmet=helmet.transform;
            experience.galleryView=galleryView; experience.cabinView=cabinView; experience.overviewView=overview;
            experience.shipView=shipView; experience.helmetView=helmetView; experience.timelineView=timelineView;
            experience.cabinRoot=cabin.gameObject;
            var narration=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/Welcome.wav");
            experience.narrationClip=narration;
            Lighting();
            var atlas=gallery.gameObject.AddComponent<MuseumTextAtlas>();atlas.labelMaterial=textMaterial;atlas.RefreshAtlas();
            MuseumObjExporter.Export(ship,"ThreeMastShip");
            MuseumObjExporter.Export(helmet,"CopperDivingHelmet");
            MuseumObjExporter.Export(cabin.gameObject,"ShipCabin");
            cabin.gameObject.SetActive(false);
            QualitySettings.SetQualityLevel(5,true); QualitySettings.antiAliasing=4;
            QualitySettings.shadowDistance=50; QualitySettings.pixelLightCount=8;
            PlayerSettings.productName="Harbour City Museum"; PlayerSettings.companyName="Harbour Museum Prototype";
            PlayerSettings.defaultScreenWidth=1600; PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.runInBackground=true;
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);
            EditorBuildSettings.scenes=new[] { new EditorBuildSettingsScene(ScenePath,true) };
            AssetDatabase.SaveAssets();
            Capture(camera, galleryView, "GalleryPreview.png");
            Capture(camera, shipView, "ShipPreview.png");
            Capture(camera, helmetView, "HelmetPreview.png");
            Capture(camera, timelineView, "StoriesPreview.png");
            cabin.gameObject.SetActive(true);
            Capture(camera,cabinView,"CabinPreview.png");
            cabin.gameObject.SetActive(false);
            camera.transform.SetPositionAndRotation(galleryView.position,galleryView.rotation);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);
            ValidateScene();
            AssetDatabase.Refresh();
            Debug.Log("HARBOUR_MUSEUM_BUILD_SUCCESS: " + ScenePath);
        }

        static void BuildGallery(Transform root)
        {
            Box("Foundation",root,new Vector3(0,-.15f,0),new Vector3(20,.3f,18),navy,true);
            for(int row=0;row<24;row++) for(int col=0;col<10;col++)
            {
                var shade=Material("Floor_Oak_"+((row+col)%4),new Color(.25f+(col%4)*.022f,.17f+(col%4)*.011f,.11f+(col%4)*.009f),.24f);
                Box("Oak floorboard",root,new Vector3(-9+col*2,-.013f,-8.63f+row*.75f),new Vector3(1.98f,.06f,.735f),shade);
            }
            Box("Back wall",root,new Vector3(0,3,9),new Vector3(20,6,.3f),wall,true);
            Box("Left wall",root,new Vector3(-10,3,0),new Vector3(.3f,6,18),wall,true);
            Box("Right wall",root,new Vector3(10,3,0),new Vector3(.3f,6,18),wall,true);
            Box("Back dado",root,new Vector3(0,.6f,8.8f),new Vector3(20,1.2f,.14f),navy);
            Box("Back brass rail",root,new Vector3(0,1.23f,8.66f),new Vector3(20,.027f,.06f),brass);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Wall dado",root,new Vector3(side*9.8f,.6f,0),new Vector3(.12f,1.2f,18),navy);
                Box("Dado brass line",root,new Vector3(side*9.69f,1.23f,0),new Vector3(.06f,.027f,18),brass);
                for(int z=-6;z<=6;z+=6)
                {
                    Box("Gallery pillar",root,new Vector3(side*9.6f,2.8f,z),new Vector3(.42f,5.6f,.42f),navy);
                    Box("Pillar capital",root,new Vector3(side*9.6f,5.48f,z),new Vector3(.72f,.2f,.72f),oak);
                }
                Box("Ceiling perimeter beam",root,new Vector3(side*8.8f,5.65f,0),new Vector3(.38f,.25f,18),oak);
                Box("Warm linear light",root,new Vector3(side*8.6f,5.5f,0),new Vector3(.045f,.035f,17.5f),glow);
            }
            for(int z=-6;z<=6;z+=6) Box("Open roof beam",root,new Vector3(0,5.8f,z),new Vector3(20,.24f,.3f),oak);
            Text("Museum wordmark",root,"HARBOUR CITY",new Vector3(0,5.08f,8.79f),.66f,new Color(.91f,.83f,.61f));
            Text("Museum subtitle",root,"M U S E U M    /    W O R K ,  W A T E R  &  P E O P L E",new Vector3(0,4.49f,8.78f),.16f,Color.white);
            // A stepped island keeps the ship visible from every side.
            Box("Ship stone base",root,new Vector3(-1,.18f,0),new Vector3(7.4f,.36f,3.45f),stone,true);
            Box("Ship island",root,new Vector3(-1,.59f,0),new Vector3(7.1f,.48f,3.2f),navy,true);
            Box("Ship oak top",root,new Vector3(-1,.88f,0),new Vector3(7.24f,.12f,3.3f),oak);
            Box("Ship brass edge",root,new Vector3(-1,.945f,0),new Vector3(7.26f,.018f,3.32f),brass);
            Text("Ship station",root,"01    THE SHIP",new Vector3(-1,.61f,-1.621f),.32f,new Color(.94f,.83f,.58f));
            Text("Ship station subtitle",root,"A PORT CITY'S WORKING TOOL",new Vector3(-1,.33f,-1.739f),.12f,Color.white);
            Text("Ship reconstruction label",root,"ORIGINAL MODEL  /  INTERPRETIVE RECONSTRUCTION",new Vector3(-1,.115f,-1.74f),.095f,new Color(.2f,.26f,.28f));
            // Helmet alcove: graphic halo and deep teal backing.
            Box("Helmet backdrop",root,new Vector3(5.15f,2.5f,2.2f),new Vector3(3.3f,4.4f,.18f),navy);
            Ring("Helmet halo",root,new Vector3(5.15f,2.8f,2.08f),1.08f,.018f,brass);
            Text("Helmet alcove title",root,"BELOW THE SURFACE",new Vector3(5.15f,4.26f,2.06f),.235f,new Color(.94f,.83f,.58f));
            Text("Helmet alcove subtitle",root,"A CLOSER LOOK AT UNDERWATER WORK",new Vector3(5.15f,3.96f,2.06f),.098f,Color.white);
            Box("Helmet pedestal foot",root,new Vector3(5.15f,.12f,.65f),new Vector3(1.9f,.24f,1.9f),brass,true);
            Box("Helmet pedestal",root,new Vector3(5.15f,.78f,.65f),new Vector3(1.8f,1.1f,1.8f),stone,true);
            Box("Helmet display top",root,new Vector3(5.15f,1.39f,.65f),new Vector3(1.92f,.12f,1.92f),navy);
            Text("Helmet number",root,"03",new Vector3(5.15f,.95f,-.262f),.4f,new Color(.11f,.26f,.28f));
            Text("Helmet pedestal title",root,"DIVING HELMET",new Vector3(5.15f,.61f,-.263f),.16f,new Color(.11f,.26f,.28f));
            Text("Helmet pedestal evidence",root,"RECONSTRUCTION",new Vector3(5.15f,.39f,-.263f),.09f,new Color(.2f,.26f,.28f));
            // A welcome desk and simple architectural seating.
            Box("Welcome desk base",root,new Vector3(-7,.58f,-4.4f),new Vector3(2.55f,1.16f,.95f),navy,true);
            Box("Welcome desk top",root,new Vector3(-7,1.22f,-4.4f),new Vector3(2.7f,.12f,1.08f),oak);
            Text("Welcome sign",root,"WELCOME ABOARD",new Vector3(-7,.82f,-4.89f),.19f,new Color(.94f,.83f,.58f));
            Text("Welcome guide",root,"POINT. SELECT. DISCOVER.",new Vector3(-7,.51f,-4.89f),.13f,Color.white);
            Text("Welcome access",root,"A FIVE-MINUTE MUSEUM VISIT",new Vector3(-7,.28f,-4.89f),.08f,Color.white);
            Bench(root,new Vector3(-7.2f,0,2),Quaternion.Euler(0,90,0));
            // Three panels with a continuous visual timeline but no invented city dates.
            string[] headings={"MARITIME WORK","PEOPLE ON THE MOVE","EVERYDAY LIFE"};
            string[] captions={"Cargo, craft and cooperation\nconnect a port to the world.","Crossings carried people,\nbelongings and new beginnings.","Waterfront communities grew\naround the rhythms of the docks."};
            MuseumAction[] actions={MuseumAction.TimelineWork,MuseumAction.TimelineMigration,MuseumAction.TimelineLife};
            string[] body={
                "A port depends on many kinds of labour. Sailors navigate and maintain a vessel; dock workers move its cargo. Inspect the ship's deck and cabin to see how objects and spaces can help explain that work. This station is a thematic reconstruction, not a record of one ship or city.",
                "Ships carry people as well as goods. Sea journeys connect migration with the work of a port and the belongings travellers carry. These stylised trunks invite discussion about departure and arrival; they do not represent a named passenger or a documented personal testimony.",
                "A port is also a place to live. Work, transport, trade and leisure bring waterfront communities into contact with ships and docks. The scene suggests those connections through everyday objects. No fictional character or event is presented as a historical fact."
            };
            Text("Stories section label",root,"02    PORT STORIES",new Vector3(-6.55f,4.07f,8.44f),.22f,new Color(.94f,.83f,.58f),TextAnchor.MiddleLeft);
            Box("Timeline line",root,new Vector3(0,1.25f,8.44f),new Vector3(14.2f,.022f,.03f),brass);
            for(int i=0;i<3;i++)
            {
                float x=-5+i*5;
                var panel=Box("Story panel / "+headings[i],root,new Vector3(x,2.64f,8.48f),new Vector3(4.15f,2.55f,.12f),navy);
                AddTarget(panel,actions[i],headings[i],body[i],Vector3.zero,new Vector3(4.15f,2.55f,.18f));
                Text("Chapter",root,"0"+(i+1)+"  /  "+headings[i],new Vector3(x,3.67f,8.38f),.17f,new Color(.94f,.83f,.58f));
                Text("Story caption",root,captions[i],new Vector3(x,1.9f,8.37f),.145f,Color.white);
                Text("Story invitation",root,"SELECT TO READ THE STORY",new Vector3(x,1.51f,8.37f),.085f,new Color(.38f,.7f,.69f));
                if(i==0) { for(int j=0;j<3;j++) Box("Relief cargo",root,new Vector3(x-.65f+j*.62f,2.55f+j*.1f,8.27f),new Vector3(.5f,.5f,.15f),oak); Line("Crane relief",root,new Vector3(x-.9f,2.35f,8.26f),new Vector3(x-.9f,3.18f,8.26f),.028f,brass); Line("Crane arm",root,new Vector3(x-.9f,3.18f,8.26f),new Vector3(x+.9f,3.18f,8.26f),.028f,brass); }
                if(i==1) { Box("Suitcase relief",root,new Vector3(x,2.73f,8.25f),new Vector3(1.5f,.8f,.17f),oak); Ring("Suitcase handle",root,new Vector3(x,3.16f,8.22f),.18f,.025f,brass); for(int j=-1;j<=1;j+=2) Box("Luggage strap",root,new Vector3(x+j*.49f,2.73f,8.14f),new Vector3(.07f,.82f,.035f),brass); }
                if(i==2) { for(int j=0;j<3;j++) { Box("Waterfront house",root,new Vector3(x-.72f+j*.72f,2.65f,8.26f),new Vector3(.58f,.67f+j*.11f,.13f),teal); Box("House window",root,new Vector3(x-.72f+j*.72f,2.72f,8.17f),new Vector3(.17f,.2f,.015f),glow); } }
            }
            Text("Evidence discipline",root,"THEMATIC STORYLINE  /  SELF-MADE ASSETS  /  NO CLAIM OF AN EXACT HISTORICAL REPLICA",new Vector3(0,.95f,8.62f),.105f,new Color(.68f,.76f,.73f));
            // Short nautical graphics on a side wall.
            var map=new GameObject("Navigation mural").transform; map.SetParent(root,false); map.localPosition=new Vector3(-9.78f,3.1f,-.6f); map.localRotation=Quaternion.Euler(0,-90,0);
            Ring("Compass outer",map,Vector3.zero,1.15f,.012f,brass); Ring("Compass inner",map,Vector3.zero,.8f,.008f,brass);
            for(int i=0;i<8;i++) { float a=i*Mathf.PI/4; Line("Compass spoke",map,new Vector3(Mathf.Sin(a)*.1f,Mathf.Cos(a)*.1f,-.02f),new Vector3(Mathf.Sin(a)*1.1f,Mathf.Cos(a)*1.1f,-.02f),.014f,brass); }
            Text("Compass north",map,"N",new Vector3(0,1.38f,0),.24f,Color.white);
            Text("Compass caption",map,"FOLLOW THE WATER",new Vector3(0,-1.6f,0),.2f,new Color(.94f,.83f,.58f));
            LightPoint("Ship warm light",root,new Vector3(-2,4.8f,-1),new Color(1,.82f,.62f),1.35f,12);
            LightPoint("Helmet warm light",root,new Vector3(5.15f,3.65f,-.7f),new Color(1,.82f,.62f),1.25f,7);
            LightPoint("Story wall wash",root,new Vector3(0,4.5f,5.5f),new Color(.75f,.87f,1),1.5f,12);
            LightPoint("Welcome light",root,new Vector3(-6,3,-5),new Color(1,.8f,.6f),.9f,8);
        }

        static void BuildCabin(Transform root)
        {
            Box("Cabin floor",root,new Vector3(0,-.12f,0),new Vector3(10,.24f,8),oak,true);
            for(int i=0;i<20;i++) Box("Cabin deck seams",root,new Vector3(-4.75f+i*.5f,.013f,0),new Vector3(.018f,.015f,8),dark);
            Box("Aft wall",root,new Vector3(0,1.7f,4),new Vector3(10,3.4f,.2f),navy,true);
            Box("Port wall",root,new Vector3(-5,1.7f,0),new Vector3(.2f,3.4f,8),oak,true);
            Box("Starboard wall",root,new Vector3(5,1.7f,0),new Vector3(.2f,3.4f,8),oak,true);
            Box("Forward wall",root,new Vector3(0,1.7f,-4),new Vector3(10,3.4f,.2f),navy,true);
            Box("Cabin roof",root,new Vector3(0,3.4f,0),new Vector3(10,.18f,8),oak);
            for(int z=-3;z<=3;z+=2) { Box("Ceiling rib",root,new Vector3(0,3.25f,z),new Vector3(10,.18f,.18f),brass); for(int s=-1;s<=1;s+=2) Box("Wall rib",root,new Vector3(s*4.84f,1.65f,z),new Vector3(.12f,3.3f,.18f),brass); }
            for(int side=-1;side<=1;side+=2) for(int z=-2;z<=2;z+=4)
            {
                var port=new GameObject("Brass porthole").transform;port.SetParent(root,false);port.localPosition=new Vector3(side*4.87f,2.08f,z);port.localRotation=Quaternion.Euler(0,side==1?-90:90,0);
                var disk=Cylinder("Sea beyond porthole",port,Vector3.zero,new Vector3(.85f,.035f,.85f),glass);disk.transform.localRotation=Quaternion.Euler(90,0,0);
                Ring("Porthole rim",port,new Vector3(0,0,-.045f),.44f,.045f,brass);
                for(int i=0;i<8;i++) { float a=i*Mathf.PI/4;Sphere("Porthole bolt",port,new Vector3(Mathf.Sin(a)*.45f,Mathf.Cos(a)*.45f,-.07f),Vector3.one*.055f,dark); }
            }
            Text("Cabin title",root,"INSIDE THE SHIP",new Vector3(0,2.88f,3.82f),.37f,new Color(.94f,.83f,.58f));
            Text("Cabin provenance",root,"INTERPRETIVE DECK SECTION / SEATING, NAVIGATION & WORK",new Vector3(0,2.46f,3.81f),.125f,Color.white);
            for(int side=-1;side<=1;side+=2)
            {
                var bench=Bench(root,new Vector3(side*3.4f,0,-1.35f),Quaternion.Euler(0,90,0));
                AddTarget(bench,MuseumAction.InteriorSeating,"Seating & shared space","Benches illustrate a shared cabin. The arrangement is designed for this prototype; it is not a documented reconstruction of a particular ship.",new Vector3(0,.5f,0),new Vector3(2.9f,1.1f,.85f));
            }
            var console=Box("Navigation console",root,new Vector3(-1.7f,.78f,2.96f),new Vector3(2.55f,1.2f,.8f),oak,true);
            AddTarget(console,MuseumAction.InteriorInstruments,"Navigation instruments","A compass and simple gauges help visitors discuss direction and observation at sea. The display combines illustrative instruments rather than reproducing a named vessel's bridge.",Vector3.zero,new Vector3(2.55f,1.4f,1));
            Text("Navigation label",root,"NAVIGATION",new Vector3(-1.7f,1.85f,3.65f),.16f,new Color(.94f,.83f,.58f));
            for(int i=0;i<3;i++)
            {
                Vector3 p=new Vector3(-2.52f+i*.8f,1.05f,2.5f);
                var d=Cylinder("Instrument face",root,p,new Vector3(.44f,.025f,.44f),white); d.transform.localRotation=Quaternion.Euler(90,0,0);
                Ring("Instrument brass bezel",root,p+new Vector3(0,0,-.05f),.245f,.025f,brass);
                Line("Gauge needle",root,p+new Vector3(-.1f,-.065f,-.06f),p+new Vector3(.09f,.12f,-.06f),.013f,dark);
                for(int tick=0;tick<8;tick++) { float a=tick*Mathf.PI/4;Line("Gauge tick",root,p+new Vector3(Mathf.Sin(a)*.17f,Mathf.Cos(a)*.17f,-.063f),p+new Vector3(Mathf.Sin(a)*.195f,Mathf.Cos(a)*.195f,-.063f),.006f,dark); }
            }
            var wheel=new GameObject("Helm wheel").transform;wheel.SetParent(root,false);wheel.localPosition=new Vector3(.85f,1.52f,2.7f);
            Box("Helm support",root,new Vector3(.85f,.6f,3),new Vector3(.25f,1.2f,.25f),oak);
            Ring("Wooden wheel",wheel,Vector3.zero,.56f,.065f,oak);
            Sphere("Wheel hub",wheel,Vector3.zero,Vector3.one*.22f,brass);
            for(int i=0;i<8;i++) { float a=i*Mathf.PI/4; Line("Helm spoke",wheel,Vector3.zero,new Vector3(Mathf.Sin(a)*.72f,Mathf.Cos(a)*.72f,0),.032f,oak); }
            var machinery=new GameObject("Working machinery | capstan + pump").transform; machinery.SetParent(root,false); machinery.localPosition=new Vector3(3.2f,0,1.2f);
            Box("Machinery platform",machinery,new Vector3(0,.12f,0),new Vector3(2.1f,.24f,2.1f),navy,true);
            Cylinder("Capstan foot",machinery,new Vector3(0,.32f,0),new Vector3(.9f,.1f,.9f),brass);
            Cylinder("Capstan drum",machinery,new Vector3(0,.82f,0),new Vector3(.55f,.45f,.55f),dark);
            Cylinder("Capstan head",machinery,new Vector3(0,1.32f,0),new Vector3(.85f,.07f,.85f),brass);
            for(int i=0;i<4;i++) { float a=i*Mathf.PI/2;Line("Capstan lever",machinery,new Vector3(0,1.34f,0),new Vector3(Mathf.Sin(a)*.93f,1.34f,Mathf.Cos(a)*.93f),.045f,oak); }
            for(int i=0;i<4;i++) { var ring=new GameObject("Rope winding").transform;ring.SetParent(machinery,false);ring.localPosition=new Vector3(0,.65f+i*.095f,0);ring.localRotation=Quaternion.Euler(90,0,0);Ring("Rope turn",ring,Vector3.zero,.295f,.025f,white); }
            AddTarget(machinery.gameObject,MuseumAction.InteriorMachinery,"Capstan / turning effort into work","A capstan is a rotating drum that helps people haul lines. This simplified reconstruction illustrates mechanical advantage and teamwork. Its placement and dimensions are demonstrative, not evidence of a specific historic vessel.",new Vector3(0,.9f,0),new Vector3(2.1f,1.8f,2.1f));
            Text("Machinery label",root,"WORKING MACHINERY",new Vector3(3.2f,2.2f,3.68f),.15f,new Color(.94f,.83f,.58f));
            for(int i=0;i<3;i++) { var crate=Box("Cargo crate",root,new Vector3(-3.8f+(i%2)*.8f,.34f+(i/2)*.65f,1.05f),new Vector3(.72f,.65f,.7f),oak);Box("Crate strap",crate.transform,Vector3.zero,new Vector3(.75f,.09f,.73f),brass); }
            Text("Cabin evidence notice",root,"SELECT THE SEATS, INSTRUMENTS OR CAPSTAN TO LEARN MORE",new Vector3(0,.31f,3.83f),.105f,new Color(.71f,.82f,.79f));
            LightPoint("Cabin warm lantern",root,new Vector3(0,2.75f,0),new Color(1,.84f,.66f),2,9);
            LightPoint("Cabin port light",root,new Vector3(-3,2.3f,0),new Color(.7f,.85f,1),.8f,6);
            LightPoint("Cabin starboard light",root,new Vector3(3,2.3f,1.5f),new Color(1,.78f,.53f),.9f,6);
        }

        static GameObject Bench(Transform parent,Vector3 position,Quaternion rotation)
        {
            var b=new GameObject("Oak bench");b.transform.SetParent(parent,false);b.transform.localPosition=position;b.transform.localRotation=rotation;
            Box("Seat",b.transform,new Vector3(0,.55f,0),new Vector3(2.8f,.16f,.7f),oak,true);
            Box("Backrest",b.transform,new Vector3(0,.93f,.3f),new Vector3(2.8f,.55f,.1f),oak);
            for(int i=-1;i<=1;i+=2) Box("Bench leg",b.transform,new Vector3(i*1.05f,.25f,0),new Vector3(.15f,.5f,.58f),dark);
            return b;
        }

        public static Material Material(string name,Color color,float smoothness=0,float metal=0)
        {
            string path=Root+"/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null) { m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path); }
            m.color=color;m.SetFloat("_Glossiness",smoothness);m.SetFloat("_Metallic",metal);EditorUtility.SetDirty(m);return m;
        }
        static GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 position,Vector3 scale,Material mat,bool collision=false)
        {
            var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;
            if(!collision) UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
        }
        static GameObject Box(string n,Transform p,Vector3 pos,Vector3 scale,Material m,bool collision=false){return Shape(n,PrimitiveType.Cube,p,pos,scale,m,collision);}
        static GameObject Sphere(string n,Transform p,Vector3 pos,Vector3 scale,Material m){return Shape(n,PrimitiveType.Sphere,p,pos,scale,m);}
        static GameObject Cylinder(string n,Transform p,Vector3 pos,Vector3 scale,Material m){return Shape(n,PrimitiveType.Cylinder,p,pos,scale,m);}
        static void Line(string n,Transform p,Vector3 a,Vector3 b,float radius,Material m)
        {
            var g=Cylinder(n,p,(a+b)*.5f,new Vector3(radius*2,(b-a).magnitude*.5f,radius*2),m);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
        }
        static void Ring(string n,Transform p,Vector3 center,float radius,float width,Material mat)
        {
            var g=new GameObject(n);g.transform.SetParent(p,false);var lr=g.AddComponent<LineRenderer>();lr.useWorldSpace=false;lr.loop=true;lr.positionCount=64;lr.widthMultiplier=width*2;lr.numCornerVertices=2;lr.sharedMaterial=mat;
            for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;lr.SetPosition(i,center+new Vector3(Mathf.Sin(a)*radius,Mathf.Cos(a)*radius,0));}
        }
        static TextMesh Text(string name,Transform parent,string content,Vector3 position,float size,Color color,TextAnchor anchor=TextAnchor.MiddleCenter)
        {
            var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;var t=g.AddComponent<TextMesh>();t.text=content;t.font=font;t.fontSize=64;t.characterSize=size*10f/64f;t.color=color;t.anchor=anchor;t.alignment=anchor==TextAnchor.MiddleLeft?TextAlignment.Left:TextAlignment.Center;
            t.GetComponent<Renderer>().sharedMaterial=textMaterial;return t;
        }
        static MuseumTarget AddTarget(GameObject g,MuseumAction action,string title,string description,Vector3 center,Vector3 size)
        {
            var target=g.AddComponent<MuseumTarget>();target.action=action;target.title=title;target.description=description;
            var col=g.AddComponent<BoxCollider>();
            Vector3 scale=g.transform.localScale;
            col.center=new Vector3(center.x/scale.x,center.y/scale.y,center.z/scale.z);
            col.size=new Vector3(size.x/Mathf.Abs(scale.x),size.y/Mathf.Abs(scale.y),size.z/Mathf.Abs(scale.z));return target;
        }
        static Transform View(string name,Transform parent,Vector3 position,Vector3 look)
        {
            var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.position=position;g.transform.rotation=Quaternion.LookRotation(look-position);return g.transform;
        }
        static void LightPoint(string name,Transform parent,Vector3 position,Color color,float intensity,float range)
        {
            var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;l.renderMode=LightRenderMode.ForcePixel;
        }
        static void Lighting()
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.42f,.48f,.5f);RenderSettings.ambientEquatorColor=new Color(.26f,.31f,.33f);RenderSettings.ambientGroundColor=new Color(.14f,.12f,.1f);
            RenderSettings.fog=false;RenderSettings.reflectionIntensity=.5f;
            var g=new GameObject("Soft daylight");g.transform.rotation=Quaternion.Euler(42,-24,0);var l=g.AddComponent<Light>();l.type=LightType.Directional;l.intensity=.72f;l.color=new Color(1,.93f,.83f);l.shadows=LightShadows.Soft;l.shadowBias=.035f;
        }
        static void Capture(Camera camera,Transform view,string name)
        {
            string directory=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"MuseumPreview");Directory.CreateDirectory(directory);
            camera.transform.SetPositionAndRotation(view.position,view.rotation);
            var previous=RenderTexture.active;var previousTarget=camera.targetTexture;
            var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);rt.antiAliasing=4;
            Texture2D tex=null;
            try
            {
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();
                File.WriteAllBytes(Path.Combine(directory,name),tex.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=previousTarget;RenderTexture.active=previous;
                rt.Release();UnityEngine.Object.DestroyImmediate(rt);if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);
            }
        }
        [MenuItem("Harbour Museum/Validate scene")]
        public static void ValidateScene()
        {
            var exp=UnityEngine.Object.FindObjectOfType<MuseumExperience>();
            if(exp==null || exp.visitorCamera==null || exp.ship==null || exp.helmet==null || exp.cabinRoot==null) throw new Exception("Missing museum references");
            int targetCount=UnityEngine.Object.FindObjectsOfType<MuseumTarget>(true).Length;
            if(targetCount<8) throw new Exception("Missing ray selection targets: "+targetCount);
            foreach(var filter in UnityEngine.Object.FindObjectsOfType<MeshFilter>(true))
                if(filter.sharedMesh==null || !EditorUtility.IsPersistent(filter.sharedMesh)) throw new Exception("Missing or unsaved mesh "+filter.name);
            foreach(var renderer in UnityEngine.Object.FindObjectsOfType<Renderer>(true))
                foreach(var m in renderer.sharedMaterials) if(m==null || m.shader==null) throw new Exception("Missing material "+renderer.name);
            Debug.Log("HARBOUR_SCENE_VALIDATED: "+targetCount+" ray targets, persistent meshes and valid materials");
        }
        [MenuItem("Harbour Museum/Build Windows prototype")]
        public static void BuildWindows()
        {
            string path=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Builds/HarbourMuseum/HarbourMuseum.exe");Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{ScenePath},locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development });
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Windows build failed: "+report.summary.result);
            Debug.Log("HARBOUR_WINDOWS_BUILD_SUCCESS: "+path);
        }
        [MenuItem("Harbour Museum/Export asset package")]
        public static void ExportPackage()
        {
            string path=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"MuseumPreview/HarbourCityMuseum.unitypackage");
            AssetDatabase.ExportPackage(Root,path,ExportPackageOptions.Recurse);
            Debug.Log("HARBOUR_ASSET_PACKAGE_SUCCESS: "+path);
        }
    }
}
