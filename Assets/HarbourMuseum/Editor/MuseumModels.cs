using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HarbourMuseum.EditorTools
{
    // Original exhibit models. Generated mesh/material assets survive scene and prefab serialization.
    public static class MuseumModels
    {
        const string Root = "Assets/HarbourMuseum";
        const string Materials = Root + "/Materials";
        const string Meshes = Root + "/Meshes";
        static readonly float[] HX = { -2.8f, -2.56f, -2.15f, -1.5f, -.75f, 0f, .85f, 1.55f, 2.08f, 2.5f, 2.8f };
        static readonly float[] HW = { .39f, .59f, .74f, .82f, .85f, .85f, .81f, .7f, .52f, .26f, .025f };
        static readonly float[] HK = { .4f, .23f, .13f, .08f, .065f, .065f, .09f, .16f, .29f, .52f, .8f };

        public static GameObject CreateShip(Transform parent = null)
        {
            Folders();
            var ship = Group("HMS Meridian · original three-masted merchant ship", parent);
            var navy = Mat("Ship_Navy", new Color(.028f, .073f, .115f), .25f, .48f);
            var keel = Mat("Ship_Keel", new Color(.025f, .036f, .041f), .12f, .4f);
            var wood = Mat("Ship_DeckWood", new Color(.49f, .265f, .105f), 0f, .36f);
            var plank = Mat("Ship_DeckPlank", new Color(.67f, .435f, .22f), 0f, .32f);
            var darkWood = Mat("Ship_DarkTimber", new Color(.18f, .105f, .052f), 0f, .3f);
            var brass = Mat("Exhibit_Brass", new Color(.73f, .48f, .18f), .75f, .54f);
            var rope = Mat("Ship_Rope", new Color(.29f, .22f, .135f), 0f, .15f);
            var canvas = Mat("Ship_Canvas", new Color(.89f, .815f, .635f), 0f, .18f);
            var seam = Mat("Ship_CanvasSeam", new Color(.64f, .55f, .37f), 0f, .16f);
            var glass = Mat("Exhibit_Glass", new Color(.105f, .33f, .37f), .35f, .88f);
            var black = Mat("Exhibit_Recess", new Color(.018f, .023f, .025f), .08f, .2f);
            var iron = Mat("Ship_Iron", new Color(.075f, .09f, .10f), .65f, .38f);
            var red = Mat("Ship_RedEnsign", new Color(.52f, .075f, .045f), 0f, .25f);
            var body = Group("01 · sculpted hull and planked decks", ship.transform).transform;
            MeshObject("Navy carvel hull", body, Hull("Ship_Hull", 0f, 1f), navy);
            MeshObject("Dark lower hull", body, Hull("Ship_LowerHull", 0f, .36f, .003f), keel);
            MeshObject("Golden sheer stripe", body, Hull("Ship_GoldSheer", .865f, .908f, .012f), brass);
            MeshObject("Lower timber wale", body, Hull("Ship_LowerWale", .46f, .50f, .012f), darkWood);
            MeshObject("Deck edge timber", body, Hull("Ship_DeckRim", .965f, 1.045f, .016f), darkWood);
            MeshObject("Main deck", body, Deck(), plank);
            for (int i = -9; i <= 9; i++)
            {
                float z = i * .078f;
                var points = new List<Vector3>();
                for (int j = 0; j < HX.Length; j++)
                    if (Mathf.Abs(z) <= HW[j] - .035f) points.Add(new Vector3(HX[j], 1.041f, z));
                if (points.Count > 1) Tube("Plank seam", body, points, .0032f, darkWood, "Ship_DeckSeam_" + (i + 10));
            }
            for (int i = 0; i < 8; i++)
            {
                float x = -2.15f + i * .56f, w = Width(x) * .85f;
                Line("Plank end joint", body, new Vector3(x, 1.042f, -w), new Vector3(x, 1.042f, w), .0025f, darkWood);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                var rail = new List<Vector3>();
                for (int i = 0; i < HX.Length - 1; i++)
                {
                    float z = side * (HW[i] - .028f);
                    Cube("Rail stanchion", body, new Vector3(HX[i], 1.16f, z), new Vector3(.035f, .24f, .035f), darkWood);
                    rail.Add(new Vector3(HX[i], 1.29f, z));
                }
                rail.Add(new Vector3(2.73f, 1.24f, side * .04f));
                Tube("Continuous gunwale rail", body, rail, .026f, darkWood, "Ship_Rail_" + side);
                for (int i = 0; i < 8; i++)
                {
                    float x = -2.16f + i * .55f, z = side * SideZ(x, .72f);
                    var port = Group((side < 0 ? "Port" : "Starboard") + " cannon port " + (i + 1), body).transform;
                    Cube("Shadowed opening", port, new Vector3(x, .72f, z), new Vector3(.205f, .148f, .025f), black);
                    Cube("Upper brass frame", port, new Vector3(x, .803f, z + side * .013f), new Vector3(.235f, .018f, .025f), brass);
                    Cube("Lower brass frame", port, new Vector3(x, .637f, z + side * .013f), new Vector3(.235f, .018f, .025f), brass);
                    for (int edge = -1; edge <= 1; edge += 2)
                        Cube("Vertical brass frame", port, new Vector3(x + edge * .112f, .72f, z + side * .012f), new Vector3(.017f, .175f, .025f), brass);
                    Line("Iron cannon muzzle", port, new Vector3(x, .719f, z), new Vector3(x, .719f, z + side * .12f), .04f, iron);
                    Line("Muzzle recess", port, new Vector3(x, .719f, z + side * .121f), new Vector3(x, .719f, z + side * .124f), .028f, black);
                }
            }
            var cabin = Group("02 · stern quarterdeck and captain's cabin", ship.transform).transform;
            Cube("Cabin", cabin, new Vector3(-2.03f, 1.20f, 0f), new Vector3(1f, .32f, 1.22f), navy);
            Cube("Raised quarterdeck", cabin, new Vector3(-2.04f, 1.395f, 0f), new Vector3(1.08f, .09f, 1.26f), plank);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    float x = -2.42f + i * .255f;
                    Cube("Cabin brass surround", cabin, new Vector3(x, 1.21f, side * .619f), new Vector3(.18f, .19f, .03f), brass);
                    Cube("Cabin glazed window", cabin, new Vector3(x, 1.21f, side * .639f), new Vector3(.145f, .15f, .016f), glass);
                    Cube("Window mullion", cabin, new Vector3(x, 1.21f, side * .650f), new Vector3(.012f, .15f, .012f), darkWood);
                }
                Line("Quarterdeck handrail", cabin, new Vector3(-2.56f, 1.61f, side * .60f), new Vector3(-1.55f, 1.61f, side * .60f), .022f, brass);
                for (int i = 0; i < 4; i++)
                    Line("Rail support", cabin, new Vector3(-2.52f + i * .31f, 1.43f, side * .60f), new Vector3(-2.52f + i * .31f, 1.61f, side * .60f), .011f, brass);
            }
            for (int i = 0; i < 5; i++)
                Cube("Quarterdeck stair", cabin, new Vector3(-1.41f + i * .075f, 1.30f - i * .055f, 0f), new Vector3(.11f, .08f, .46f), wood);
            Wheel(cabin, new Vector3(-1.88f, 1.64f, 0f), darkWood, brass);
            Cube("Transom gilt ornament", cabin, new Vector3(-2.584f, 1.19f, 0f), new Vector3(.026f, .27f, .98f), brass);
            Cube("Transom inlay", cabin, new Vector3(-2.604f, 1.19f, 0f), new Vector3(.025f, .20f, .87f), navy);
            for (int i = 0; i < 5; i++)
                Cube("Aft glazed light", cabin, new Vector3(-2.627f, 1.20f, -.34f + i * .17f), new Vector3(.025f, .14f, .12f), glass);

            var rigging = Group("03 · masts, curved canvas and standing rigging", ship.transform).transform;
            float[] mx = { -1.30f, .18f, 1.53f }, tops = { 3.25f, 3.67f, 3.12f };
            for (int m = 0; m < 3; m++)
            {
                float x = mx[m], top = tops[m];
                var mast = Group(new[] { "Mizzen mast", "Main mast", "Foremast" }[m], rigging).transform;
                Line("Lower timber mast", mast, new Vector3(x, 1.04f, 0f), new Vector3(x, top - .6f, 0f), .055f, wood);
                Line("Upper spar", mast, new Vector3(x, top - .75f, 0f), new Vector3(x, top, 0f), .028f, darkWood);
                for (int i = 0; i < 5; i++)
                    Ring("Mast binding", mast, new Vector3(x, 1.10f + i * .32f, 0f), .059f, .009f, iron, "Ship_MastHoop", Quaternion.Euler(90f, 0f, 0f));
                Prim("Lookout platform", PrimitiveType.Cylinder, mast, new Vector3(x, top - .64f, 0f), new Vector3(.30f, .035f, .30f), darkWood);
                Ring("Lookout rim", mast, new Vector3(x, top - .57f, 0f), .135f, .016f, darkWood, "Ship_NestRim", Quaternion.Euler(90f, 0f, 0f));
                for (int tier = 0; tier < 3; tier++)
                {
                    float y = top - .30f - tier * .65f;
                    float w = (tier == 0 ? .83f : tier == 1 ? 1.29f : 1.62f) * (m == 1 ? 1f : .85f);
                    float h = tier == 0 ? .40f : tier == 1 ? .53f : .57f;
                    Line("Yard " + (tier + 1), mast, new Vector3(x, y + .025f, -w * .56f), new Vector3(x, y + .025f, w * .56f), tier == 2 ? .022f : .017f, darkWood);
                    MeshObject("Billowed canvas " + (tier + 1), mast, Sail("Ship_Sail_" + m + "_" + tier, x, y, w, h), canvas);
                    for (int s = -2; s <= 2; s++)
                    {
                        var points = new List<Vector3>();
                        for (int step = 0; step <= 12; step++)
                            points.Add(SailPoint(x, y, w, h, s * .33f, step / 12f) + new Vector3(.0025f, 0f, 0f));
                        Tube("Canvas stitched seam", mast, points, .0016f, seam, "Ship_SailSeam_" + m + "_" + tier + "_" + s);
                    }
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Line("Yard brace", mast, new Vector3(x, y, side * w * .55f), new Vector3(x - .34f, 1.20f, side * Width(x) * .88f), .0045f, rope);
                        Line("Sail sheet", mast, SailPoint(x, y, w, h, side, 1f), new Vector3(x + .34f, 1.10f, side * Width(x) * .85f), .004f, rope);
                    }
                }
                for (int side = -1; side <= 1; side += 2)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float dx = x - .28f + i * .16f;
                        Line("Standing shroud", mast, new Vector3(x, top - .67f, side * .035f), new Vector3(dx, 1.10f, side * (Width(dx) - .07f)), .0045f, rope);
                    }
                    for (int rung = 0; rung < 12; rung++)
                    {
                        float t = .08f + rung * .056f;
                        Vector3 a = Vector3.Lerp(new Vector3(x - .28f, 1.10f, side * (Width(x) - .07f)), new Vector3(x, top - .67f, side * .035f), t);
                        Vector3 b = Vector3.Lerp(new Vector3(x + .20f, 1.10f, side * (Width(x) - .07f)), new Vector3(x, top - .67f, side * .035f), t);
                        Line("Ratline ladder rung", mast, a, b, .0026f, rope);
                    }
                }
                MeshObject("Mast pennant", mast, Flag("Ship_Pennant_" + m, new Vector3(x, top - .02f, 0f), .40f, .14f), red);
            }
            Line("Bowsprit", rigging, new Vector3(2.1f, 1.10f, 0f), new Vector3(3.48f, 1.63f, 0f), .035f, darkWood);
            Line("Fore royal stay", rigging, new Vector3(1.53f, 3.12f, 0f), new Vector3(3.48f, 1.63f, 0f), .005f, rope);
            Line("Fore lower stay", rigging, new Vector3(1.53f, 2.47f, 0f), new Vector3(3.48f, 1.63f, 0f), .004f, rope);
            Vector3 ja = new Vector3(1.65f, 2.85f, 0f), jb = new Vector3(3.36f, 1.66f, 0f), jc = new Vector3(1.89f, 1.48f, 0f);
            MeshObject("Triangular jib", rigging, SaveMesh("Ship_Jib", new List<Vector3> { ja, jb, jc, ja, jb, jc }, new List<int> { 0, 1, 2, 5, 4, 3 }), canvas);
            Line("Main forestay", rigging, new Vector3(.18f, 3.6f, 0f), new Vector3(1.53f, 2.75f, 0f), .004f, rope);
            Line("Mizzen forestay", rigging, new Vector3(-1.30f, 3.19f, 0f), new Vector3(.18f, 2.7f, 0f), .004f, rope);
            Line("Aft stay", rigging, new Vector3(-1.30f, 3.18f, 0f), new Vector3(-2.58f, 1.5f, 0f), .004f, rope);
            Line("Stern ensign staff", rigging, new Vector3(-2.42f, 1.43f, 0f), new Vector3(-2.67f, 2.10f, 0f), .017f, darkWood);
            MeshObject("Stern red ensign", rigging, Flag("Ship_SternFlag", new Vector3(-2.67f, 2.10f, 0f), .48f, .25f), red);

            var cargo = Group("04 · deck furniture and cargo", ship.transform).transform;
            for (int i = 0; i < 6; i++)
                Crate(cargo, new Vector3(-.74f + (i / 2) * .59f, 1.17f, i % 2 == 0 ? -.37f : .37f), wood, darkWood, brass, i);
            for (int i = 0; i < 3; i++)
            {
                var item = Group("Oak water barrel " + (i + 1), cargo).transform;
                item.localPosition = new Vector3(.72f + i * .26f, 1.04f, .15f);
                MeshObject("Coopered barrel", item, Lathe("Ship_Barrel", new Vector2[] { new Vector2(.085f, 0f), new Vector2(.11f, .05f), new Vector2(.12f, .14f), new Vector2(.11f, .24f), new Vector2(.085f, .28f) }, 24), wood);
                for (int ring = 0; ring < 3; ring++)
                    Ring("Iron barrel hoop", item, new Vector3(0f, .04f + ring * .10f, 0f), ring == 1 ? .12f : .107f, .008f, iron, "Ship_BarrelHoop_" + ring, Quaternion.Euler(90f, 0f, 0f));
                Prim("Barrel lid", PrimitiveType.Cylinder, item, new Vector3(0f, .278f, 0f), new Vector3(.17f, .005f, .17f), darkWood);
            }
            Cube("Cargo hatch rim", cargo, new Vector3(-.5f, 1.075f, 0f), new Vector3(.48f, .06f, .44f), darkWood);
            for (int i = 0; i < 6; i++)
                Cube("Hatch grating", cargo, new Vector3(-.69f + i * .076f, 1.112f, 0f), new Vector3(.018f, .025f, .38f), wood);
            Prim("Capstan drum", PrimitiveType.Cylinder, cargo, new Vector3(1.95f, 1.16f, 0f), new Vector3(.17f, .12f, .17f), darkWood);
            for (int i = 0; i < 4; i++)
                Line("Capstan handspike", cargo, new Vector3(1.95f, 1.29f, 0f), new Vector3(1.95f + Mathf.Cos(i * Mathf.PI * .5f) * .31f, 1.29f, Mathf.Sin(i * Mathf.PI * .5f) * .31f), .012f, wood);
            for (int side = -1; side <= 1; side += 2)
                Anchor(cargo, new Vector3(2.18f, .93f, side * .48f), side, iron, rope);
            RemoveColliders(ship);
            return ship;
        }

        public static GameObject CreateHelmet(Transform parent = null)
        {
            Folders();
            var helmet = Group("Mark V · original copper diving helmet", parent);
            var copper = Mat("Helmet_AgedCopper", new Color(.53f, .27f, .105f), .8f, .48f);
            var brass = Mat("Helmet_PolishedBrass", new Color(.81f, .57f, .25f), .82f, .65f);
            var dark = Mat("Helmet_DarkCopper", new Color(.24f, .13f, .065f), .7f, .36f);
            var rubber = Mat("Helmet_RubberGasket", new Color(.045f, .043f, .039f), 0f, .28f);
            var glass = Mat("Helmet_SeaGlass", new Color(.075f, .21f, .24f), .44f, .91f);
            var hose = Mat("Helmet_AirHose", new Color(.18f, .15f, .10f), 0f, .26f);
            var shoulder = Group("01 · shoulder corselet and neck seal", helmet.transform).transform;
            var seat = MeshObject("Shaped shoulder plate", shoulder, Lathe("Helmet_ShoulderPlate", new Vector2[] {
                new Vector2(0f, .025f), new Vector2(.35f, .025f), new Vector2(.54f, .065f), new Vector2(.56f, .10f),
                new Vector2(.49f, .15f), new Vector2(.32f, .25f), new Vector2(.29f, .28f) }, 72), copper);
            seat.transform.localScale = new Vector3(1f, 1f, .73f);
            var edge = Ring("Rolled shoulder plate edge", shoulder, new Vector3(0f, .09f, 0f), .535f, .018f, brass, "Helmet_ShoulderRim", Quaternion.Euler(90f, 0f, 0f));
            edge.transform.localScale = new Vector3(1f, .73f, 1f);
            Prim("Rubber neck seal", PrimitiveType.Cylinder, shoulder, new Vector3(0f, .257f, 0f), new Vector3(.61f, .025f, .61f), rubber);
            Prim("Heavy neck locking collar", PrimitiveType.Cylinder, shoulder, new Vector3(0f, .295f, 0f), new Vector3(.69f, .035f, .69f), dark);
            Ring("Bright neck locking ring", shoulder, new Vector3(0f, .319f, 0f), .337f, .023f, brass, "Helmet_NeckRing", Quaternion.Euler(90f, 0f, 0f));
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2f / 12f;
                Bolt(shoulder, new Vector3(Mathf.Cos(a) * .43f, .162f, Mathf.Sin(a) * .30f), Vector3.up, .027f, brass, "Shoulder fixing " + (i + 1));
            }
            for (int side = -1; side <= 1; side += 2)
            {
                Cube("Breastplate weighted fastening", shoulder, new Vector3(side * .36f, .132f, -.26f), new Vector3(.13f, .042f, .115f), dark);
                Bolt(shoulder, new Vector3(side * .36f, .16f, -.26f), Vector3.up, .033f, brass, "Weighted wing nut");
            }
            var dome = Group("02 · spun copper dome", helmet.transform).transform;
            MeshObject("Continuous copper shell", dome, Lathe("Helmet_Dome", new Vector2[] {
                new Vector2(.287f, .325f), new Vector2(.344f, .37f), new Vector2(.396f, .45f), new Vector2(.429f, .56f),
                new Vector2(.429f, .68f), new Vector2(.405f, .80f), new Vector2(.357f, .90f), new Vector2(.284f, .986f),
                new Vector2(.189f, 1.045f), new Vector2(.091f, 1.080f), new Vector2(0f, 1.094f) }, 96), copper);
            Ring("Rolled shell seam", dome, new Vector3(0f, .369f, 0f), .343f, .010f, dark, "Helmet_ShellSeam", Quaternion.Euler(90f, 0f, 0f));
            for (int i = 0; i < 18; i++)
            {
                float a = i * Mathf.PI * 2f / 18f;
                Prim("Copper shell rivet", PrimitiveType.Sphere, dome, new Vector3(Mathf.Cos(a) * .352f, .395f, Mathf.Sin(a) * .352f), Vector3.one * .018f, brass);
            }
            var ports = Group("03 · glass ports, removable bezels and guards", helmet.transform).transform;
            Viewport(ports, new Vector3(0f, .680f, -.414f), Quaternion.identity, .181f, brass, dark, glass, true, "Front viewing port");
            Viewport(ports, new Vector3(-.411f, .678f, .01f), Quaternion.Euler(0f, 90f, 0f), .138f, brass, dark, glass, true, "Left viewing port");
            Viewport(ports, new Vector3(.411f, .678f, .01f), Quaternion.Euler(0f, -90f, 0f), .138f, brass, dark, glass, true, "Right viewing port");
            Viewport(ports, new Vector3(0f, 1.081f, -.015f), Quaternion.Euler(90f, 0f, 0f), .070f, brass, dark, glass, false, "Top inspection port");
            var fittings = Group("04 · air supply, exhaust and lifting eye", helmet.transform).transform;
            Line("Air intake elbow stem", fittings, new Vector3(.25f, .79f, .28f), new Vector3(.34f, .87f, .37f), .051f, brass);
            Line("Air intake hose connector", fittings, new Vector3(.34f, .87f, .37f), new Vector3(.34f, .96f, .37f), .053f, dark);
            for (int i = 0; i < 4; i++)
                Ring("Threaded airline connector", fittings, new Vector3(.34f, .89f + i * .018f, .37f), .053f, .005f, brass, "Helmet_HoseThread", Quaternion.Euler(90f, 0f, 0f));
            Tube("Short air supply hose", fittings, new List<Vector3> {
                new Vector3(.34f, .97f, .37f), new Vector3(.38f, 1.03f, .43f), new Vector3(.45f, .99f, .49f),
                new Vector3(.48f, .87f, .52f), new Vector3(.45f, .69f, .55f), new Vector3(.39f, .44f, .56f),
                new Vector3(.32f, .18f, .56f) }, .034f, hose, "Helmet_Hose");
            for (int i = 0; i < 13; i++)
                Ring("Air hose rib", fittings, Vector3.Lerp(new Vector3(.46f, .90f, .515f), new Vector3(.33f, .21f, .56f), i / 12f),
                    .035f, .004f, dark, "Helmet_HoseRib", Quaternion.Euler(90f, 0f, 10f));
            Line("Exhaust valve housing", fittings, new Vector3(-.27f, .48f, .23f), new Vector3(-.38f, .45f, .33f), .065f, brass);
            Line("Exhaust release button", fittings, new Vector3(-.38f, .45f, .33f), new Vector3(-.41f, .44f, .36f), .043f, dark);
            Ring("Lifting eye", fittings, new Vector3(0f, 1.13f, .08f), .047f, .012f, brass, "Helmet_LiftingEye", Quaternion.Euler(0f, 90f, 0f));
            Cube("Lifting eye solder foot", fittings, new Vector3(0f, 1.086f, .08f), new Vector3(.08f, .018f, .067f), dark);
            RemoveColliders(helmet);
            return helmet;
        }

        static void Viewport(Transform parent, Vector3 position, Quaternion rotation, float radius,
            Material brass, Material copper, Material glass, bool grid, string name)
        {
            var root = Group(name, parent).transform;
            root.localPosition = position;
            root.localRotation = rotation;
            Prim("Dark recessed glass", PrimitiveType.Sphere, root, new Vector3(0f, 0f, -.014f), new Vector3(radius * 1.92f, radius * 1.92f, .032f), glass);
            Ring("Cast copper socket", root, Vector3.zero, radius + .015f, .034f, copper, "Helmet_Socket_" + radius, Quaternion.identity);
            Ring("Polished retaining bezel", root, new Vector3(0f, 0f, -.023f), radius, .024f, brass, "Helmet_Bezel_" + radius, Quaternion.identity);
            Ring("Inner glass gasket", root, new Vector3(0f, 0f, -.033f), radius - .026f, .005f, copper, "Helmet_GlassGasket_" + radius, Quaternion.identity);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * .25f;
                Bolt(root, new Vector3(Mathf.Cos(a) * (radius + .011f), Mathf.Sin(a) * (radius + .011f), -.052f), Vector3.back, .014f, brass, "Bezel screw " + (i + 1));
            }
            if (!grid) return;
            float clear = radius - .036f;
            for (int line = -1; line <= 1; line++)
            {
                float offset = line * clear * .50f, length = Mathf.Sqrt(clear * clear - offset * offset);
                Line("Protective vertical grille", root, new Vector3(offset, -length, -.053f), new Vector3(offset, length, -.053f), .006f, brass);
                Line("Protective horizontal grille", root, new Vector3(-length, offset, -.055f), new Vector3(length, offset, -.055f), .006f, brass);
            }
        }

        static void Bolt(Transform parent, Vector3 p, Vector3 normal, float r, Material material, string name)
        {
            var go = MeshObject(name, parent, Lathe("Exhibit_HexBolt_" + r, new Vector2[] {
                new Vector2(r, 0f), new Vector2(r, r * .6f), new Vector2(r * .8f, r * .78f), new Vector2(0f, r * .78f) }, 6), material);
            go.transform.localPosition = p;
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal);
        }

        static void Crate(Transform parent, Vector3 p, Material wood, Material frame, Material metal, int index)
        {
            var crate = Group("Cargo crate " + (index + 1), parent).transform;
            crate.localPosition = p;
            Cube("Planked wooden crate", crate, Vector3.zero, new Vector3(.31f, .25f, .28f), wood);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int edge = -1; edge <= 1; edge += 2)
                {
                    Cube("Vertical batten", crate, new Vector3(edge * .118f, 0f, side * .143f), new Vector3(.025f, .25f, .017f), frame);
                    Cube("Edge strap", crate, new Vector3(side * .158f, edge * .0975f, 0f), new Vector3(.012f, .022f, .28f), frame);
                }
                var brace = Cube("Diagonal crate brace", crate, new Vector3(0f, 0f, side * .155f), new Vector3(.30f, .025f, .015f), frame);
                brace.transform.localRotation = Quaternion.Euler(0f, 0f, 36f);
                Cube("Lid metal strap", crate, new Vector3(side * .099f, .129f, 0f), new Vector3(.017f, .009f, .28f), metal);
            }
        }

        static void Wheel(Transform parent, Vector3 p, Material timber, Material brass)
        {
            var wheel = Group("Eight-spoke ship's wheel", parent).transform;
            wheel.localPosition = p;
            wheel.localRotation = Quaternion.Euler(0f, 90f, 0f);
            Ring("Wheel rim", wheel, Vector3.zero, .145f, .016f, timber, "Ship_WheelRim", Quaternion.identity);
            Line("Wheel axle", wheel, new Vector3(0f, 0f, -.04f), new Vector3(0f, 0f, .04f), .03f, brass);
            for (int i = 0; i < 8; i++)
                Line("Wheel spoke and handle", wheel, Vector3.zero, new Vector3(Mathf.Cos(i * Mathf.PI * .25f) * .19f, Mathf.Sin(i * Mathf.PI * .25f) * .19f, 0f), .011f, timber);
            Cube("Helm pedestal", parent, p + new Vector3(0f, -.14f, 0f), new Vector3(.065f, .26f, .075f), timber);
        }

        static void Anchor(Transform parent, Vector3 p, int side, Material iron, Material rope)
        {
            var anchor = Group("Forged anchor " + (side < 0 ? "port" : "starboard"), parent).transform;
            anchor.localPosition = p;
            Ring("Anchor eye", anchor, Vector3.zero, .040f, .009f, iron, "Ship_AnchorEye", Quaternion.identity);
            Line("Anchor shank", anchor, Vector3.zero, new Vector3(0f, -.28f, 0f), .017f, iron);
            Line("Anchor stock", anchor, new Vector3(-.13f, -.07f, 0f), new Vector3(.13f, -.07f, 0f), .016f, iron);
            Tube("Curved crown", anchor, new List<Vector3> {
                new Vector3(-.13f, -.19f, 0f), new Vector3(-.10f, -.27f, 0f), new Vector3(0f, -.30f, 0f),
                new Vector3(.10f, -.27f, 0f), new Vector3(.13f, -.19f, 0f) }, .017f, iron, "Ship_AnchorCrown");
            for (int f = -1; f <= 1; f += 2)
            {
                var blade = Cube("Anchor fluke", anchor, new Vector3(f * .123f, -.205f, 0f), new Vector3(.07f, .06f, .038f), iron);
                blade.transform.localRotation = Quaternion.Euler(0f, 0f, f * -25f);
            }
            Line("Anchor hauling line", parent, p, new Vector3(p.x - .2f, 1.18f, side * .45f), .008f, rope);
        }

        static float Width(float x)
        {
            for (int i = 0; i < HX.Length - 1; i++)
                if (x <= HX[i + 1]) return Mathf.Lerp(HW[i], HW[i + 1], Mathf.InverseLerp(HX[i], HX[i + 1], x));
            return HW[HW.Length - 1];
        }

        static float SideZ(float x, float y)
        {
            float keel = .1f;
            for (int i = 0; i < HX.Length - 1; i++)
                if (x <= HX[i + 1])
                {
                    keel = Mathf.Lerp(HK[i], HK[i + 1], Mathf.InverseLerp(HX[i], HX[i + 1], x));
                    break;
                }
            float f = Mathf.Clamp01((y - keel) / (1f - keel));
            return Width(x) * Mathf.Sqrt(1f - (1f - f) * (1f - f));
        }

        static Mesh Hull(string name, float from, float to, float offset = 0f)
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            const int sections = 20;
            for (int side = -1; side <= 1; side += 2)
            {
                int start = v.Count;
                for (int i = 0; i < HX.Length; i++)
                    for (int j = 0; j <= sections; j++)
                    {
                        float f = Mathf.Lerp(from, to, j / (float)sections);
                        float z = f > 1f ? HW[i] : HW[i] * Mathf.Sqrt(Mathf.Max(0f, 1f - (1f - f) * (1f - f)));
                        v.Add(new Vector3(HX[i], HK[i] + (1f - HK[i]) * f, side * (z + offset)));
                    }
                for (int i = 0; i < HX.Length - 1; i++)
                    for (int j = 0; j < sections; j++)
                    {
                        int a = start + i * (sections + 1) + j;
                        if (side < 0) Quad(t, a, a + 1, a + sections + 2, a + sections + 1);
                        else Quad(t, a, a + sections + 1, a + sections + 2, a + 1);
                    }
            }
            int cap = v.Count;
            for (int j = 0; j <= sections; j++)
            {
                float f = Mathf.Lerp(from, to, j / (float)sections), c = Mathf.Min(1f, f);
                float z = HW[0] * Mathf.Sqrt(Mathf.Max(0f, 1f - (1f - c) * (1f - c)));
                float y = HK[0] + (1f - HK[0]) * f;
                v.Add(new Vector3(HX[0], y, -z - offset));
                v.Add(new Vector3(HX[0], y, z + offset));
            }
            for (int j = 0; j < sections; j++) Quad(t, cap + j * 2, cap + j * 2 + 1, cap + j * 2 + 3, cap + j * 2 + 2);
            return SaveMesh(name, v, t);
        }

        static Mesh Deck()
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            for (int i = 0; i < HX.Length; i++)
            {
                v.Add(new Vector3(HX[i], 1.035f, -HW[i]));
                v.Add(new Vector3(HX[i], 1.035f, HW[i]));
            }
            for (int i = 0; i < HX.Length - 1; i++) Quad(t, i * 2, i * 2 + 1, i * 2 + 3, i * 2 + 2);
            return SaveMesh("Ship_MainDeck", v, t);
        }

        static Vector3 SailPoint(float x, float top, float w, float h, float u, float row)
        {
            float swell = Mathf.Sin((u + 1f) * Mathf.PI * .5f) * Mathf.Sin(row * Mathf.PI);
            return new Vector3(x + .065f + swell * .22f, top - h * row - .035f * (1f - u * u) * row, u * w * .5f * (1f - .09f * row));
        }

        static Mesh Sail(string name, float x, float top, float w, float h)
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            const int cols = 20, rows = 12;
            for (int face = 0; face < 2; face++)
            {
                int start = v.Count;
                for (int row = 0; row <= rows; row++)
                    for (int col = 0; col <= cols; col++)
                        v.Add(SailPoint(x, top, w, h, col / (float)cols * 2f - 1f, row / (float)rows));
                for (int row = 0; row < rows; row++)
                    for (int col = 0; col < cols; col++)
                    {
                        int a = start + row * (cols + 1) + col;
                        if (face == 0) Quad(t, a, a + 1, a + cols + 2, a + cols + 1);
                        else Quad(t, a, a + cols + 1, a + cols + 2, a + 1);
                    }
            }
            return SaveMesh(name, v, t);
        }

        static Mesh Flag(string name, Vector3 p, float width, float height)
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            for (int face = 0; face < 2; face++)
            {
                int start = v.Count;
                for (int col = 0; col <= 8; col++)
                {
                    float u = col / 8f, wave = Mathf.Sin(u * Mathf.PI * 2f) * .04f;
                    v.Add(p + new Vector3(-width * u, -u * .025f, wave));
                    v.Add(p + new Vector3(-width * u, -height - u * .025f + Mathf.Sin(u * Mathf.PI) * .03f, wave));
                }
                for (int col = 0; col < 8; col++)
                {
                    int a = start + col * 2;
                    if (face == 0) Quad(t, a, a + 2, a + 3, a + 1);
                    else Quad(t, a, a + 1, a + 3, a + 2);
                }
            }
            return SaveMesh(name, v, t);
        }

        static Mesh Lathe(string name, Vector2[] profile, int steps)
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            for (int row = 0; row < profile.Length; row++)
                for (int seg = 0; seg <= steps; seg++)
                {
                    float a = seg * Mathf.PI * 2f / steps;
                    v.Add(new Vector3(Mathf.Cos(a) * profile[row].x, profile[row].y, Mathf.Sin(a) * profile[row].x));
                }
            for (int row = 0; row < profile.Length - 1; row++)
                for (int seg = 0; seg < steps; seg++)
                {
                    int a = row * (steps + 1) + seg;
                    Quad(t, a, a + steps + 1, a + steps + 2, a + 1);
                }
            return SaveMesh(name, v, t);
        }

        static GameObject Ring(string name, Transform parent, Vector3 p, float major, float minor, Material mat, string meshName, Quaternion rotation)
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            const int around = 48, section = 10;
            for (int a = 0; a <= around; a++)
                for (int b = 0; b <= section; b++)
                {
                    float u = a * Mathf.PI * 2f / around, w = b * Mathf.PI * 2f / section;
                    float r = major + Mathf.Cos(w) * minor;
                    v.Add(new Vector3(Mathf.Cos(u) * r, Mathf.Sin(u) * r, Mathf.Sin(w) * minor));
                }
            for (int a = 0; a < around; a++)
                for (int b = 0; b < section; b++)
                {
                    int i = a * (section + 1) + b;
                    Quad(t, i, i + section + 1, i + section + 2, i + 1);
                }
            var go = MeshObject(name, parent, SaveMesh(meshName, v, t), mat);
            go.transform.localPosition = p;
            go.transform.localRotation = rotation;
            return go;
        }

        static GameObject Tube(string name, Transform parent, IList<Vector3> points, float radius, Material mat, string meshName)
        {
            var v = new List<Vector3>();
            var t = new List<int>();
            const int sides = 8;
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 direction = i == 0 ? points[1] - points[0] : i == points.Count - 1 ? points[i] - points[i - 1] : points[i + 1] - points[i - 1];
                direction.Normalize();
                Vector3 tangent = Vector3.Cross(direction, Mathf.Abs(direction.y) > .9f ? Vector3.right : Vector3.up).normalized;
                Vector3 bitangent = Vector3.Cross(direction, tangent).normalized;
                for (int j = 0; j < sides; j++)
                {
                    float a = j * Mathf.PI * 2f / sides;
                    v.Add(points[i] + (tangent * Mathf.Cos(a) + bitangent * Mathf.Sin(a)) * radius);
                }
            }
            for (int i = 0; i < points.Count - 1; i++)
                for (int j = 0; j < sides; j++)
                {
                    int a = i * sides + j, b = i * sides + (j + 1) % sides;
                    Quad(t, a, b, b + sides, a + sides);
                }
            return MeshObject(name, parent, SaveMesh(meshName, v, t), mat);
        }

        static GameObject Line(string name, Transform parent, Vector3 a, Vector3 b, float radius, Material mat)
        {
            Vector3 delta = b - a;
            var go = Prim(name, PrimitiveType.Cylinder, parent, (a + b) * .5f, new Vector3(radius * 2f, delta.magnitude * .5f, radius * 2f), mat);
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            return go;
        }

        static GameObject Cube(string name, Transform parent, Vector3 p, Vector3 scale, Material mat)
        {
            return Prim(name, PrimitiveType.Cube, parent, p, scale, mat);
        }

        static GameObject Prim(string name, PrimitiveType type, Transform parent, Vector3 p, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = p;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            var collider = go.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            return go;
        }

        static GameObject Group(string name, Transform parent)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        static GameObject MeshObject(string name, Transform parent, Mesh mesh, Material mat)
        {
            var go = Group(name, parent);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        static Mesh SaveMesh(string name, List<Vector3> vertices, List<int> triangles)
        {
            var mesh = new Mesh { name = name };
            if (vertices.Count > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            var uv = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count; i++) uv[i] = new Vector2(vertices[i].x + vertices[i].z, vertices[i].y);
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            string path = Meshes + "/" + name.Replace('.', '_').Replace('-', '_').Replace(',', '_') + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mesh, existing);
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(mesh);
                return existing;
            }
            if (!AssetDatabase.Contains(mesh)) AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        static Material Mat(string name, Color color, float metallic, float smoothness)
        {
            string path = Materials + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard")) { name = name };
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void Quad(List<int> t, int a, int b, int c, int d)
        {
            t.Add(a); t.Add(b); t.Add(c);
            t.Add(a); t.Add(c); t.Add(d);
        }

        static void Folders()
        {
            if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets", "HarbourMuseum");
            if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Root, "Materials");
            if (!AssetDatabase.IsValidFolder(Meshes)) AssetDatabase.CreateFolder(Root, "Meshes");
        }

        static void RemoveColliders(GameObject root)
        {
            foreach (var c in root.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
        }
    }
}
