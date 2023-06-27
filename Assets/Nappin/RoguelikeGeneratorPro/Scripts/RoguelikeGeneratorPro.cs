using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;


namespace RoguelikeGeneratorPro
{
    /// <summary>
    /// 'tileType'은 게임 레벨의 각 타일이 가질 수 있는 타입을 나열합니다.
    /// </summary>
    public enum tileType
    {
        empty,  // 타일이 비어있음을 나타냅니다.
        floor,  // 타일이 바닥임을 나타냅니다.
        wall,   // 타일이 벽임을 나타냅니다.
        detail  // 타일에 상세 정보(디테일)가 있음을 나타냅니다.
    };
    

    /// <summary>
    /// 'overlayType'은 타일에 적용할 수 있는 오버레이 타입을 나열합니다.
    /// </summary>
    public enum overlayType
    {
        empty,        // 오버레이가 없음을 나타냅니다.
        wallPattern,  // 벽에 대한 패턴 오버레이를 나타냅니다.
        wallRandom,   // 벽에 대한 무작위 오버레이를 나타냅니다.
        floorPattern, // 바닥에 대한 패턴 오버레이를 나타냅니다.
        floorRandom   // 바닥에 대한 무작위 오버레이를 나타냅니다.
    };

    /// <summary>
    /// 'patternType'은 오버레이 패턴의 종류를 나열합니다.
    /// </summary>
    public enum patternType
    {
        perlinNoise,// 텍스처, 지형 및 자연스럽고 유기적인 모양이 필요한 기타 요소를 생성하기 위해 절차적 콘텐츠 생성에 자주 사용됩니다.
        checker,// 바둑판 패턴은 바둑판처럼 두 가지 색상(또는 이 경우 타일 유형)이 번갈아 나타납니다.
        wideChecker,// 이 부재는 넓은 체크 무늬를 사용함을 나타냅니다. 일반 체커 패턴과 비슷하지만 '사각형'이 더 큽니다. 이로 인해 교체 횟수가 줄어듭니다.
        lineLeft,// 이 멤버는 왼쪽에서 시작하는 선 패턴을 사용함을 나타냅니다. 패턴은 타일을 가로질러 왼쪽에서 시작하여 오른쪽으로 이동하는 선처럼 보입니다.
        lineRight// 이 멤버는 오른쪽에서 시작하는 선 패턴을 사용함을 나타냅니다. 패턴은 오른쪽에서 시작하여 왼쪽으로 이동하면서 타일을 가로지르는 선처럼 보입니다.
    };
 

    /// <summary>
    /// 'levelRotation'은 레벨 회전의 방향을 나열합니다.
    /// </summary>
    public enum levelRotation
    {
        XZ,  // XZ 평면을 따라 레벨을 회전시킵니다.
        XY,  // XY 평면을 따라 레벨을 회전시킵니다.
        ZY   // ZY 평면을 따라 레벨을 회전시킵니다.
    };

    /// <summary>
    /// 'genType'은 레벨 생성시 사용할 수 있는 메소드를 나열합니다.
    /// </summary>
    public enum genType
    {
        generateObj,   // 오브젝트 생성 메소드를 나타냅니다.
        generateTile,  // 타일 생성 메소드를 나타냅니다.
        noGeneration   // 생성 없음을 나타냅니다.
    };

    public struct pathMaker
    {
        public Vector2 direction;
        public Vector2 position;
    };
    
    public class RoguelikeGeneratorPro : MonoBehaviour
    {
        #region Variables
        
        //Level dimensions
        public Vector2Int levelSize = new Vector2Int(80, 80); // 레벨의 크기(너비와 높이)
        public float tileSize = 1f; // 타일의 크기
        public float percentLevelToFill = 25.3f; // 채울 레벨의 백분율
        public bool spawnCornerWalls = true; // 모서리에 벽을 생성할 지 여부
        public bool removeUnnaturalWalls = false; // 비자연스러운 벽을 제거할 지 여부
        public bool useSeed = false; // 시드를 사용할 지 여부
        public int generationSeed = 99; // 생성에 사용되는 시드 값

        //Chunk properties
        public int chunkSpawnChance = 4; // 청크가 생성될 확률
        public float chunkChance2x2 = 95; // 2x2 청크가 생성될 확률
        public float chunkChance3x3 = 5; // 3x3 청크가 생성될 확률
        
        //PathMaker 속성
        public int pathMakerDestructionChance = 30; // PathMaker가 파괴될 확률
        public int pathMakerSpawnChance = 39; // PathMaker가 생성될 확률
        public int pathMakerRotationChance = 44; // PathMaker가 회전할 확률
        public float pathMakerRotatesLeft = 8.080808f; // PathMaker가 왼쪽으로 회전할 확률
        public float pathMakerRotatesRight = 38.38384f; // PathMaker가 오른쪽으로 회전할 확률
        public float pathMakerRotatesBackwards = 53.53535f; // PathMaker가 뒤로 회전할 확률
        public int pathMakerMaxDensity = 2; // PathMaker의 최대 밀도
        private List<pathMaker> pathMakers; // 경로 생성자(PathMaker)의 리스트
        
        //  Pattern floor overlay
        // 이 변수는 바닥에 사용할 패턴을 지정합니다. 사용할 패턴을 지정하기 위해 'patternType' 열거형을 사용합니다.
        public patternType patternFloor = patternType.checker;

        // 이 변수는 바닥 패턴에 대한 Perlin 노이즈를 생성하는 데 사용할 노이즈의 크기를 나타내는 2D 벡터입니다.
        // 값이 클수록 그래디언트가 더 크고 부드러워지고 값이 작을수록 더 미세하고 복잡한 패턴이 생성됩니다.
        public Vector2 noiseScaleFloor = new Vector2(0.1f, 0.1f);

        // 이 변수는 패턴의 일부로 간주되어야 하는 Perlin 노이즈 함수의 값을 결정하는 데 사용되는 임계값입니다.
        // 값이 높을수록 패턴에 포함되는 바닥 부분이 줄어들고 값이 낮을수록 패턴에 포함되는 바닥 부분이 많아집니다.
        public float noiseCutoffFloor = 0.5f;
        
        //Pattern wall overlay
        public patternType patternWall = patternType.checker;
        public Vector2 noiseScaleWall = new Vector2(0.1f, 0.1f);
        public float noiseCutoffWall = 0.5f;
        
        public int randomFloorOverlayChance = 1; // 맵 생성 과정에서 바닥 타일이 'floorRandom' 오버레이로 오버레이될 확률(100/100). 
        public int randomWallOverlayChance = 1;// 맵 생성 프로세스 중에 벽 타일이 'wallRandom' 오버레이로 오버레이될 확률(100/100). 
        
        //Tiles generation references
        public int tabSelected = 0;
        public genType generation = 0;
        public bool fillAllTiles = false;
        public bool drawCorners = false;
        
        public bool drawEmptyTiles = true;   // 그리드에 빈 타일을 그릴지 여부를 나타내는 부울 변수입니다. true인 경우 그리드는 emptyTileObj에서 지정한 대로 빈 타일을 포함합니다.
        public GameObject emptyTileObj;   // 그리드의 빈 타일에 대한 템플릿으로 사용되는 GameObject입니다. 이 개체는 빈 타일이 필요할 때마다 인스턴스화되어 그리드에 배치됩니다.
        
        public bool drawFloorOverlayPatternTiles = true;    // 패턴 바닥 오버레이 타일을 그려야 하는지 여부를 나타내는 플래그입니다. true인 경우 패턴 오버레이 타일이 바닥에 그려집니다.
        public GameObject patternFloorTileObj;    // 이것은 패턴 바닥 타일의 GameObject입니다. 이 타일 객체는 패턴 바닥 오버레이 타일을 그릴 때 사용됩니다.
        
        public bool drawFloorOverlayRandomTiles = true;   // 임의의 바닥 오버레이 타일을 그려야 하는지 여부를 나타내는 플래그입니다. 참이면 무작위 오버레이 타일이 바닥에 그려집니다.
        public GameObject randomFloorTileObj;     // 이것은 임의의 바닥 타일에 대한 GameObject입니다. 이 타일 개체는 임의의 바닥 오버레이 타일을 그릴 때 사용됩니다.


        public bool drawTilesOrientation = true;
        public GameObject floorTileObj_1;
        public GameObject floorTileObj_2;
        public GameObject floorTileObj_3;
        public GameObject floorTileObj_4;
        public GameObject floorTileObj_5;
        public GameObject floorTileObj_6;
        public GameObject floorTileObj_7;
        public GameObject floorTileObj_8;
        public GameObject floorTileObj_9;
        public GameObject floorTileObj_10;
        public GameObject floorTileObj_11;
        public GameObject floorTileObj_12;
        public GameObject floorTileObj_13;
        public GameObject floorTileObj_14;
        public GameObject floorTileObj_15;
        
        public GameObject floorTileObj_C1;
        public GameObject floorTileObj_C2;
        public GameObject floorTileObj_C3;
        public GameObject floorTileObj_C4;
        public GameObject floorTileObj_C5;
        public GameObject floorTileObj_C6;
        public GameObject floorTileObj_C7;
        public GameObject floorTileObj_C8;
        public GameObject floorTileObj_C9;
        public GameObject floorTileObj_C10;
        public GameObject floorTileObj_C11;
        public GameObject floorTileObj_C12;
        public GameObject floorTileObj_C13;
        public GameObject floorTileObj_C14;
        public GameObject floorTileObj_C15;
        public GameObject floorTileObj_C16;
        public GameObject floorTileObj_C17;
        public GameObject floorTileObj_C18;
        public GameObject floorTileObj_C19;
        public GameObject floorTileObj_C20;
        public GameObject floorTileObj_C21;
        public GameObject floorTileObj_C22;
        public GameObject floorTileObj_C23;
        public GameObject floorTileObj_C24;
        public GameObject floorTileObj_C25;
        public GameObject floorTileObj_C26;
        public GameObject floorTileObj_C27;
        public GameObject floorTileObj_C28;
        public GameObject floorTileObj_C29;
        public GameObject floorTileObj_C30;
        public GameObject floorTileObj_C31;

        public GameObject wallTileObj_1;
        public GameObject wallTileObj_2;
        public GameObject wallTileObj_3;
        public GameObject wallTileObj_4;
        public GameObject wallTileObj_5;
        public GameObject wallTileObj_6;
        public GameObject wallTileObj_7;
        public GameObject wallTileObj_8;
        public GameObject wallTileObj_9;
        public GameObject wallTileObj_10;
        public GameObject wallTileObj_11;
        public GameObject wallTileObj_12;
        public GameObject wallTileObj_13;
        public GameObject wallTileObj_14;
        public GameObject wallTileObj_15;

        public GameObject wallTileObj_C1;
        public GameObject wallTileObj_C2;
        public GameObject wallTileObj_C3;
        public GameObject wallTileObj_C4;
        public GameObject wallTileObj_C5;
        public GameObject wallTileObj_C6;
        public GameObject wallTileObj_C7;
        public GameObject wallTileObj_C8;
        public GameObject wallTileObj_C9;
        public GameObject wallTileObj_C10;
        public GameObject wallTileObj_C11;
        public GameObject wallTileObj_C12;
        public GameObject wallTileObj_C13;
        public GameObject wallTileObj_C14;
        public GameObject wallTileObj_C15;
        public GameObject wallTileObj_C16;
        public GameObject wallTileObj_C17;
        public GameObject wallTileObj_C18;
        public GameObject wallTileObj_C19;
        public GameObject wallTileObj_C20;
        public GameObject wallTileObj_C21;
        public GameObject wallTileObj_C22;
        public GameObject wallTileObj_C23;
        public GameObject wallTileObj_C24;
        public GameObject wallTileObj_C25;
        public GameObject wallTileObj_C26;
        public GameObject wallTileObj_C27;
        public GameObject wallTileObj_C28;
        public GameObject wallTileObj_C29;
        public GameObject wallTileObj_C30;
        public GameObject wallTileObj_C31;

        
        public bool drawWallOverlayPatternTiles = true; // 패턴 타일을 벽 타일 위에 그릴지 여부를 나타내는 부울 변수입니다. true인 경우 패턴 벽 오버레이에 대해 정의된 설정에 따라 벽 타일 위에 패턴이 적용됩니다.
        public GameObject patternWallTileObj_1;
        public GameObject patternWallTileObj_2;
        public GameObject patternWallTileObj_3;
        public GameObject patternWallTileObj_4;
        public GameObject patternWallTileObj_5;
        public GameObject patternWallTileObj_6;
        public GameObject patternWallTileObj_7;
        public GameObject patternWallTileObj_8;
        public GameObject patternWallTileObj_9;
        public GameObject patternWallTileObj_10;
        public GameObject patternWallTileObj_11;
        public GameObject patternWallTileObj_12;
        public GameObject patternWallTileObj_13;
        public GameObject patternWallTileObj_14;
        public GameObject patternWallTileObj_15;

       
        public bool drawWallOverlayRandomTiles = true;  // 벽 타일 위에 임의의 타일을 그릴지 여부를 나타내는 부울 변수입니다. true인 경우 정의된 세트에서 무작위 타일을 선택하고 벽 타일 위에 적용합니다.
        public GameObject randomWallTileObj_1;
        public GameObject randomWallTileObj_2;
        public GameObject randomWallTileObj_3;
        public GameObject randomWallTileObj_4;
        public GameObject randomWallTileObj_5;
        public GameObject randomWallTileObj_6;
        public GameObject randomWallTileObj_7;
        public GameObject randomWallTileObj_8;
        public GameObject randomWallTileObj_9;
        public GameObject randomWallTileObj_10;
        public GameObject randomWallTileObj_11;
        public GameObject randomWallTileObj_12;
        public GameObject randomWallTileObj_13;
        public GameObject randomWallTileObj_14;
        public GameObject randomWallTileObj_15;

        public Tile emptyTile;
        public Tile patternFloorTile;
        public Tile randomFloorTile;

        public Tile floorTile_1;
        public Tile floorTile_2;
        public Tile floorTile_3;
        public Tile floorTile_4;
        public Tile floorTile_5;
        public Tile floorTile_6;
        public Tile floorTile_7;
        public Tile floorTile_8;
        public Tile floorTile_9;
        public Tile floorTile_10;
        public Tile floorTile_11;
        public Tile floorTile_12;
        public Tile floorTile_13;
        public Tile floorTile_14;
        public Tile floorTile_15;
        
        public Tile floorTile_C1;
        public Tile floorTile_C2;
        public Tile floorTile_C3;
        public Tile floorTile_C4;
        public Tile floorTile_C5;
        public Tile floorTile_C6;
        public Tile floorTile_C7;
        public Tile floorTile_C8;
        public Tile floorTile_C9;
        public Tile floorTile_C10;
        public Tile floorTile_C11;
        public Tile floorTile_C12;
        public Tile floorTile_C13;
        public Tile floorTile_C14;
        public Tile floorTile_C15;
        public Tile floorTile_C16;
        public Tile floorTile_C17;
        public Tile floorTile_C18;
        public Tile floorTile_C19;
        public Tile floorTile_C20;
        public Tile floorTile_C21;
        public Tile floorTile_C22;
        public Tile floorTile_C23;
        public Tile floorTile_C24;
        public Tile floorTile_C25;
        public Tile floorTile_C26;
        public Tile floorTile_C27;
        public Tile floorTile_C28;
        public Tile floorTile_C29;
        public Tile floorTile_C30;
        public Tile floorTile_C31;

        public Tile wallTile_1;
        public Tile wallTile_2;
        public Tile wallTile_3;
        public Tile wallTile_4;
        public Tile wallTile_5;
        public Tile wallTile_6;
        public Tile wallTile_7;
        public Tile wallTile_8;
        public Tile wallTile_9;
        public Tile wallTile_10;
        public Tile wallTile_11;
        public Tile wallTile_12;
        public Tile wallTile_13;
        public Tile wallTile_14;
        public Tile wallTile_15;

        public Tile wallTile_C1;
        public Tile wallTile_C2;
        public Tile wallTile_C3;
        public Tile wallTile_C4;
        public Tile wallTile_C5;
        public Tile wallTile_C6;
        public Tile wallTile_C7;
        public Tile wallTile_C8;
        public Tile wallTile_C9;
        public Tile wallTile_C10;
        public Tile wallTile_C11;
        public Tile wallTile_C12;
        public Tile wallTile_C13;
        public Tile wallTile_C14;
        public Tile wallTile_C15;
        public Tile wallTile_C16;
        public Tile wallTile_C17;
        public Tile wallTile_C18;
        public Tile wallTile_C19;
        public Tile wallTile_C20;
        public Tile wallTile_C21;
        public Tile wallTile_C22;
        public Tile wallTile_C23;
        public Tile wallTile_C24;
        public Tile wallTile_C25;
        public Tile wallTile_C26;
        public Tile wallTile_C27;
        public Tile wallTile_C28;
        public Tile wallTile_C29;
        public Tile wallTile_C30;
        public Tile wallTile_C31;

        public Tile patternWallTile_1;
        public Tile patternWallTile_2;
        public Tile patternWallTile_3;
        public Tile patternWallTile_4;
        public Tile patternWallTile_5;
        public Tile patternWallTile_6;
        public Tile patternWallTile_7;
        public Tile patternWallTile_8;
        public Tile patternWallTile_9;
        public Tile patternWallTile_10;
        public Tile patternWallTile_11;
        public Tile patternWallTile_12;
        public Tile patternWallTile_13;
        public Tile patternWallTile_14;
        public Tile patternWallTile_15;

        public Tile randomWallTile_1;
        public Tile randomWallTile_2;
        public Tile randomWallTile_3;
        public Tile randomWallTile_4;
        public Tile randomWallTile_5;
        public Tile randomWallTile_6;
        public Tile randomWallTile_7;
        public Tile randomWallTile_8;
        public Tile randomWallTile_9;
        public Tile randomWallTile_10;
        public Tile randomWallTile_11;
        public Tile randomWallTile_12;
        public Tile randomWallTile_13;
        public Tile randomWallTile_14;
        public Tile randomWallTile_15;


        //Level floor collider
        public bool createLevelSizedFloorCollider = false;
        public bool createWall2DCompositeCollider = false;
        public bool deleteFloorBelowOverlay = false;
        public bool createWallGridCollider = true;
        public float levelColliderHeight = 0.1f;
        public levelRotation levelRot;


        //Offset
        public float floorOffset = 0f; // 일반적으로 그리드의 원점을 기준으로 바닥의 수직 위치를 조정하는 데 사용되는 바닥 GameObject의 위치에 대한 오프셋입니다.
        public float wallOffset = 0f; // 일반적으로 그리드의 원점을 기준으로 벽의 수직 위치를 조정하는 데 사용되는 벽 GameObject의 위치에 대한 오프셋입니다.
        public float overlayOffset = 0.03f; // 오버레이 게임 오브젝트의 위치에 대한 오프셋으로, 일반적으로 그리드의 원점을 기준으로 오버레이의 수직 위치(예: 바닥이나 벽의 장식 또는 추가 레이어)를 조정하는 데 사용됩니다.
        public float emptyOffset = 0f; // 빈 게임 오브젝트의 위치에 대한 오프셋으로, 일반적으로 그리드의 원점을 기준으로 빈 타일(예: 간격 또는 걸을 수 없는 영역)의 수직 위치를 조정하는 데 사용됩니다.
        
        private tileType[,] tiles; // 2차원 배열로 게임 레벨의 모든 타일을 저장. 각 타일은 'tileType' 열거형 값으로 나타냄.
        private overlayType[,] overlayTiles; // 2차원 배열로 게임 레벨의 모든 오버레이 타일을 저장. 각 오버레이 타일은 'overlayType' 열거형 값으로 나타냄.
        
      
        private Vector2Int levelSizeCut; // 잘라낸 레벨의 크기
        private float iterationsMax = 100000; // 최대 반복 횟수

        private GameObject gridParent;      // 부모 오브젝트, 이 오브젝트 아래에 모든 그리드 관련 오브젝트들이 위치합니다.
        private GameObject floorParent;     // 바닥 타일 오브젝트들을 담기 위한 부모 오브젝트입니다.
        private GameObject wallParent;      // 벽 타일 오브젝트들을 담기 위한 부모 오브젝트입니다.
        private GameObject emptyParent;     // 빈 타일 오브젝트들을 담기 위한 부모 오브젝트입니다.
        private GameObject overlayParent;   // 오버레이 타일 오브젝트들을 담기 위한 부모 오브젝트입니다.
        
        #endregion
        

        #region Generation

        /// <summary>
        /// 이전에 생성된 레벨 요소를 지우고 시드 값을 할당하고 새 레벨을 생성하고 로그 메시지를 인쇄합니다.
        /// </summary>
        public void ReGenerateLevel()
        {
            Clear(); // 이전에 생성된 레벨 요소들을 제거
            AssignSeed(); // 시드 값 할당
            GenerateLevel(); // 레벨 생성
            Debug.Log("Level generated!"); // 로그 메시지 출력
        }


        public void GenerateLevel()
        {
            Setup();    // 기본 설정을 준비합니다.
            
            GenerateFloor();    // 바닥을 생성합니다.
            
            GenerateWall(); // 벽을 생성합니다.

            // 랜덤 오버레이 바닥 타일을 그릴지에 따라 생성합니다.
            if (drawFloorOverlayRandomTiles) 
                InstantiateFloorRandomOverlay();
            
            // 랜덤 오버레이 벽 타일을 그릴지에 따라 생성합니다.
            if (drawWallOverlayRandomTiles)
                InstantiateWallRandomOverlay();
            
            // 패턴 오버레이 바닥 타일을 그릴지에 따라 생성합니다.
            if (drawFloorOverlayPatternTiles)
                InstantiateFloorOverlay();
            
            // 패턴 오버레이 벽 타일을 그릴지에 따라 생성합니다.
            if (drawWallOverlayPatternTiles) 
                InstanciateWallOverlay();

            // 생성 타입에 따라 스폰을 합니다.
            if(generation != genType.noGeneration)
                Spawn();

            // 오브젝트 생성 타입이고, 바닥 콜라이더 생성 옵션이 활성화 된 경우 바닥 콜라이더를 생성합니다.
            if (generation == genType.generateObj && createLevelSizedFloorCollider) 
                GenerateFloorCollider();
            
            // 타일 생성 타입이고, 벽 그리드 콜라이더 생성 옵션이 활성화 된 경우 벽 그리드 콜라이더를 생성합니다.
            else if (generation == genType.generateTile && createWallGridCollider)
                GenerateWallTileCollider();

            // 오브젝트 생성 타입이고, 2D 벽 복합 콜라이더 생성 옵션이 활성화 된 경우 2D 벽 복합 콜라이더를 생성합니다.
            if (generation == genType.generateObj && createWall2DCompositeCollider)
                GenerateWallCompositeCollider2D();
        }

        #endregion


        #region Setup
        
        /// <summary>
        /// 크기 설정, 회전 및 블록 확률 정규화, 회전 초기화, 레벨에 대한 부모 개체 생성, 타일 초기화 및 첫 번째 경로 생성기를 생성하여 레벨을 설정합니다.
        /// </summary>
        private void Setup()
        {
            SetupLevelSize(); // 레벨의 크기를 설정합니다. 이는 레벨 경계에서 벽을 생성하기 위해 주어진 레벨 크기에서 일부를 차감합니다. 
            
            NormalizeRotationProbabilities(); // 회전 확률을 다시 계산합니다. 각 값이 비례적으로 스케일링되어 총합이 100이 되게 합니다.
            
            NormalizeBlockProbabilities(); // 2x2와 3x3 블록 확률을 다시 계산합니다. 각 값이 비례적으로 스케일링되어 총합이 100이 되게 합니다.
            
            this.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // 회전을 초기화합니다.
            
            CreateLevelParent(); // 레벨의 부모 객체를 생성합니다.
            
            InitializeTiles(); // 타일을 초기화하고 모든 타일 및 오버레이 타일을 빈 상태로 설정합니다.
            
            CreateFirstPathMaker();// 첫 번째 경로 생성기를 생성합니다.
        }
        
        /// <summary>
        /// 주어진 레벨의 크기를 조정하여 레벨 경계에 벽을 만들고 레벨 크기가 4 이상인지 확인하여 게임 로직 중에 예외를 방지함으로써 레벨 크기를 설정합니다.
        /// </summary>
        private void SetupLevelSize()
        {
            // 주어진 레벨의 크기를 조정하여 레벨 경계에 벽을 생성합니다.
            levelSizeCut = new Vector2Int(levelSize.x - 2, levelSize.y - 2);

            // 게임 로직 중에 예외를 피하기 위해 레벨 크기가 4 이상인지 확인하십시오.
            if (levelSizeCut.x < 4)
            {
                levelSizeCut.x = 4;
                levelSize.x = 6;
            }
            if (levelSizeCut.y < 4)
            {
                levelSizeCut.y = 4;
                levelSize.y = 6;
            }
        }

        /// <summary>
        /// 좌회전, 우회전, 후진의 총 확률을 계산한 다음 각 회전의 확률을 전체 확률의 백분율로 재설정하여 회전 확률을 정규화합니다.
        /// </summary>
        private void NormalizeRotationProbabilities()
        {
            // 좌회전, 우회전, 후진의 총 확률 계산
            float totalChances = pathMakerRotatesLeft + pathMakerRotatesRight + pathMakerRotatesBackwards;

            // 각 회전의 확률을 총 확률의 백분율로 재설정
            pathMakerRotatesLeft = pathMakerRotatesLeft * 100f / totalChances;
            pathMakerRotatesRight = pathMakerRotatesRight * 100f / totalChances;
            pathMakerRotatesBackwards = pathMakerRotatesBackwards * 100f / totalChances;
        }

        /// <summary>
        /// 2x2 및 3x3 청크에 대한 총 블록 기회를 계산한 다음 각 블록 크기의 확률을 총 블록 기회의 백분율로 재설정하여 블록 확률을 정규화합니다.
        /// </summary>
        private void NormalizeBlockProbabilities()
        {
            float totalBlockChances = chunkChance2x2 + chunkChance3x3;
            chunkChance2x2 = chunkChance2x2 * 100 / totalBlockChances;
            chunkChance3x3 = chunkChance3x3 * 100 / totalBlockChances;
        }
        
        /// <summary>
        /// 수준에 대한 부모 개체를 만듭니다. 생성 유형에 따라 객체 상위 또는 타일 상위를 생성합니다.
        /// </summary>
        private void CreateLevelParent()
        {
            if (generation == genType.generateObj)
                CreateObjParents();
            else
                CreateTilesParents();
        }

        /// <summary>
        /// 모든 타일과 오버레이 타일을 빈 상태로 설정하여 타일과 오버레이 타일을 초기화합니다.
        /// </summary>
        private void InitializeTiles()
        {
            tiles = new tileType[levelSize.x, levelSize.y];
            overlayTiles = new overlayType[levelSize.x, levelSize.y];

            // 모든 타일 및 오버레이 타일을 빈 상태로 초기화
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    tiles[x, y] = tileType. empty;
                    overlayTiles[x, y] = overlayType. empty;
                }
            }
        }

        /// <summary>
        /// 레벨 중앙에 새 pathMaker를 추가하여 첫 번째 경로 생성기를 만듭니다.
        /// </summary>
        private void CreateFirstPathMaker()
        {
            pathMakers = new List<pathMaker>();
            pathMaker newGenerator = new pathMaker
            {
                direction = TurnPathMakers(Vector2.up),
                position = new Vector2(Mathf.RoundToInt(levelSizeCut.x / 2.0f), Mathf.RoundToInt(levelSizeCut.y / 2.0f))
            };
            pathMakers. Add(newGenerator);
        }
        
        /// <summary>
        /// 바닥, 벽, 빈 공간 및 오버레이(필요한 경우)에 대한 개체 부모를 만듭니다.
        /// </summary>
        private void CreateObjParents()
        {
            // 도우미 메서드를 사용하여 상위 게임 개체를 만들고 설정합니다.
            floorParent = CreateParentObject("floorParent", floorOffset);
            wallParent = CreateParentObject("wallParent", wallOffset);

            // 빈 공간에 대한 부모 생성 및 필요한 경우 오버레이
            if (drawEmptyTiles)
            {
                emptyParent = CreateParentObject("emptyParent", emptyOffset);
            }

            if (drawFloorOverlayPatternTiles || drawFloorOverlayRandomTiles || drawWallOverlayPatternTiles || drawWallOverlayRandomTiles)
            {
                overlayParent = CreateParentObject("overlayParent", overlayOffset);
            }
        }
        
        /// <summary>
        /// 주어진 이름과 오프셋으로 새 부모 게임 개체를 만들고 이 변환의 자식으로 설정합니다.
        /// </summary>
        /// <param name="name">상위 게임 개체의 이름입니다.</param>
        /// <param name="offset">부모 게임 개체의 로컬 위치에 적용할 오프셋입니다.</param>
        /// <returns>생성된 상위 게임 개체입니다.</returns>
        private GameObject CreateParentObject(string name, float offset)
        {
            // 새 게임 개체를 만들고 부모 및 로컬 위치를 설정합니다.
            GameObject obj = new GameObject(name)
            {
                transform =
                {
                    parent = this.transform,
                    localPosition = new Vector3(0f, offset, 0f)
                }
            };
            return obj;
        }
        
        /// <summary>
        /// 그리드, 바닥, 벽, 빈 공간 및 오버레이(필요한 경우)에 대한 타일 부모를 만듭니다.
        /// </summary>
        private void CreateTilesParents()
        {
            gridParent = new GameObject("gridParent");
            gridParent.AddComponent<Grid>();
            gridParent.GetComponent<Grid>().cellSwizzle = GridLayout.CellSwizzle.XYZ;
            gridParent.GetComponent<Grid>().cellSize = new Vector3(tileSize, tileSize, tileSize);
            gridParent.transform.parent = this.transform;

            floorParent = CreateTileParent("floorParent", floorOffset);
            wallParent = CreateTileParent("wallParent", wallOffset);

            if (drawEmptyTiles)
            {
                emptyParent = CreateTileParent("emptyParent", emptyOffset);
            }

            if (drawFloorOverlayPatternTiles || drawFloorOverlayRandomTiles || drawWallOverlayPatternTiles || drawWallOverlayRandomTiles)
            {
                overlayParent = CreateTileParent("overlayParent", overlayOffset);
                overlayParent.GetComponent<TilemapRenderer>().sortingOrder = 1;
            }
        }
        
        /// <summary>
        /// 주어진 이름과 오프셋으로 새로운 타일 상위 게임 오브젝트를 생성하고 이를 그리드 상위 변환의 하위로 설정합니다.
        /// </summary>
        /// <param name="tileName">타일 상위 게임 개체의 이름입니다.</param>
        /// <param name="offset">타일 상위 게임 개체의 로컬 위치에 적용할 오프셋입니다.</param>
        /// <returns>생성된 타일 상위 게임 개체입니다.</returns>
        private GameObject CreateTileParent(string tileName, float offset)
        {
            GameObject tileParent = new GameObject(tileName);
            tileParent.AddComponent<Tilemap>();
            tileParent.AddComponent<TilemapRenderer>();
            tileParent.GetComponent<Tilemap>().orientation = Tilemap.Orientation.XY;
            tileParent.transform.parent = gridParent.transform;
            tileParent.transform.position = new Vector3(0f, 0f, offset);
            return tileParent;
        }
        
        /// <summary>
        /// 이전에 생성된 레벨 요소들을 지웁니다.
        /// </summary>
        private void Clear()
        {
            int childNum = this.transform.childCount;
            if (Application.isPlaying)
            {
                for (var i = childNum - 1; i >= 0; i--)
                {
                    GameObject.Destroy(this.transform.GetChild(i).gameObject);
                }
            }
            else
            {
                for (var i = childNum - 1; i >= 0; i--)
                {
                    GameObject.DestroyImmediate(this.transform.GetChild(i).gameObject);
                }
            }
        }
        
        /// <summary>
        /// Seed를 할당합니다.
        /// </summary>
        private void AssignSeed()
        {
            if (useSeed) 
                Random.InitState(generationSeed);
        }

        #endregion


        #region Floor

        /// <summary>
        /// 바닥을 생성합니다.
        /// </summary>
        private void GenerateFloor()
        {
            int iterationsNum = 0;

            // 최대 반복 한계에 도달할 때까지 지속적으로 바닥 생성
            while (iterationsNum < iterationsMax)
            {
                //층 지정
                for (int i = 0; i < pathMakers.Count; i++)
                {
                    tiles[(int)pathMakers[i].position.x, (int)pathMakers[i].position.y] = tileType.floor;
                }

                // pathMakers의 도움으로 바닥 타일 추가 생성
                IteratePathMakers(); // pathMaker에서 반복을 수행합니다.
                GenerateBlock(); // 바닥 타일의 블록(2x2 또는 3x3)을 생성합니다.
                MovePathMakers(); // 다음 반복을 위해 pathMaker를 이동합니다.

                // 생성된 바닥 타일이 이미 레벨의 특정 비율을 덮고 있는지 확인합니다. 그렇다면 루프를 끊으십시오.
                if ((float)TileTypeNumber(tileType.floor) * 100f / (float)tiles.Length > percentLevelToFill) 
                    break;
                
                iterationsNum++;
            }
        }
        
        /// <summary>
        /// 모든 경로 제작자를 반복하고 해당 조건에 따라 작업을 실행합니다.
        /// </summary>
        private void IteratePathMakers()
        {
            // 모든 경로 제작자를 반복합니다.
            for (int i = pathMakers.Count - 1; i >= 0; i--)
            {
                if (DestroyPathMaker(i)) { continue; } // 조건이 충족되면 경로 제작자를 파괴합니다.
                RotatePathMaker(i); // 조건이 충족되면 경로 제작자를 회전시킵니다.
                SpawnPathMaker(i); // 조건이 충족되면 경로 제작자를 생성합니다.
            }
        }
        
        /// <summary>
        /// 조건이 충족되면 경로 제작자를 파괴합니다.
        /// </summary>
        private bool DestroyPathMaker(int destroyIndex)
        {
            // 경로 제작자가 둘 이상이고 무작위로 생성된 숫자가 파괴 확률보다 적은 경우에만 파괴하십시오.
            if (Random.Range(0, 100) < pathMakerDestructionChance && pathMakers.Count > 1)
            {
                pathMakers.RemoveAt(destroyIndex);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// 조건이 충족되면 경로 제작자를 회전시킵니다.
        /// </summary>
        private void RotatePathMaker(int index)
        {
            // 임의로 생성된 숫자가 회전 확률보다 적으면 경로 제작자의 방향을 변경합니다.
            if (Random.Range(0, 100) < pathMakerRotationChance)
            {
                pathMaker currentPathMaker = pathMakers[index];
                currentPathMaker.direction = TurnPathMakers(currentPathMaker.direction);
                pathMakers[index] = currentPathMaker;
            }
        }
        
        /// <summary>
        /// 조건이 충족되면 새로운 경로 제작자를 생성합니다.
        /// </summary>
        private void SpawnPathMaker(int index)
        {
            // 임의로 생성된 숫자가 스폰 확률보다 적고 총 경로 제작자 수가 최대 밀도보다 작은 경우 새 경로 제작자를 만듭니다.
            if (Random.Range(0, 100) < pathMakerSpawnChance && pathMakers.Count < pathMakerMaxDensity)
            {
                pathMaker newPathMaker = new pathMaker();
                newPathMaker.direction = TurnPathMakers(pathMakers[index].direction);
                newPathMaker.position = pathMakers[index].position;
                pathMakers.Add(newPathMaker);
            }
        }
        
        
        /// <summary>
        /// 임의의 기회를 기반으로 경로 제작자의 위치에서 블록을 생성합니다.
        /// </summary>
        private void GenerateBlock()
        {
            // 각 경로 제작자를 반복합니다.
            for (int i = 0; i < pathMakers.Count; i++)
            {
                // 패스 메이커의 위치에 블록이 생성되어야 하는지 확인합니다.
                if ( (Random.Range(0, 100) < chunkSpawnChance) == false)
                {
                    continue;
                }
                
                // 경로 제작자의 위치를 가져옵니다.
                pathMaker currentPathMaker = pathMakers[i];
                int x = (int)currentPathMaker.position.x;
                int y = (int)currentPathMaker.position.y;

                // 생성할 블록 유형을 확인합니다: 2x2 또는 3x3.
                if (Random.Range(0, 100) < chunkChance2x2)
                {
                    // 경로 작성기 위치에 2x2 블록을 만듭니다.
                    SetTileType(x + 1, y, tileType.floor);
                    SetTileType(x, y + 1, tileType.floor);
                    SetTileType(x + 1, y + 1, tileType.floor);
                }
                // 경로 제작자의 위치에 3x3 블록을 생성합니다.
                else  
                {
                    for (int dx = 0; dx < 3; dx++)
                    {
                        for (int dy = 0; dy < 3; dy++)
                        {
                            SetTileType(x + dx, y + dy, tileType.floor);
                        }
                    }//for end
                }
            }
        }

        /// <summary>
        /// 지정된 위치의 타일에 특정 타일 유형을 설정합니다.
        /// </summary>
        /// <param name="x">타일의 x 좌표입니다.</param>
        /// <param name="y">타일의 y 좌표입니다.</param>
        /// <param name="newTileType">타일에 할당할 새 유형.</param>
        private void SetTileType(int x, int y, tileType newTileType)
        {
            tiles[x, y] = newTileType;
        }

        /// <summary>
        /// pathMaker의 방향을 변경합니다.
        /// </summary>
        private void MovePathMakers()
        {
            // 모든 경로 생성자를 순회합니다.
            for (int i = 0; i < pathMakers.Count; i++)
            {
                // 현재 경로 생성자를 선택하고 이동시킵니다.
                pathMaker currentPathMaker = pathMakers[i];
                currentPathMaker.position += currentPathMaker.direction;

                // 경로 생성자가 지정된 경계를 벗어나지 않도록 합니다.
                // 경로 생성자의 위치를 level의 크기 내에 제한함으로써 경로 생성자가 경계 밖으로 이동하는 것을 방지합니다.
                currentPathMaker.position.x = Mathf.Clamp(currentPathMaker.position.x, 1, levelSizeCut.x - 2);
                currentPathMaker.position.y = Mathf.Clamp(currentPathMaker.position.y, 1, levelSizeCut.y - 2);
                
                // 업데이트된 경로 생성자의 정보를 리스트에 다시 저장합니다.
                pathMakers[i] = currentPathMaker;
            }
        }


        /// <summary>
        /// PathMaker의 방향을 전환합니다.
        /// </summary>
        /// <param name="_pathMakerDirection"></param>
        /// <returns></returns>
        private Vector2 TurnPathMakers(Vector2 _pathMakerDirection)
        {
            // 0부터 100까지의 무작위 값 생성
            int randomValue = Random.Range(0, 100);

            // 왼쪽으로 회전할 확률 설정
            float chanceLeft = pathMakerRotatesLeft;
            
            // 오른쪽으로 회전할 확률 설정. 왼쪽으로 회전할 확률에 오른쪽으로 회전할 확률을 더함
            float chanceRight = chanceLeft + pathMakerRotatesRight;

            // 생성된 랜덤값이 왼쪽으로 회전할 확률보다 작거나 같은 경우
            if (randomValue <= chanceLeft)
            {
                // 현재 방향에 따라 다음 방향을 왼쪽으로 설정
                if (_pathMakerDirection == Vector2.up) return Vector2.left;
                if (_pathMakerDirection == Vector2.left) return Vector2.down;
                if (_pathMakerDirection == Vector2.down) return Vector2.right;
                return Vector2.up;
            }
            
            // 생성된 랜덤값이 오른쪽으로 회전할 확률보다 작거나 같은 경우
            if (randomValue <= chanceRight)
            {
                // 현재 방향에 따라 다음 방향을 오른쪽으로 설정
                if (_pathMakerDirection == Vector2.up) return Vector2.right;
                if (_pathMakerDirection == Vector2.left) return Vector2.up;
                if (_pathMakerDirection == Vector2.down) return Vector2.left;
                return Vector2.down;
            }
            
            // 그 외의 경우(즉, 랜덤값이 뒤로 회전할 확률 범위에 해당하는 경우)
            if (_pathMakerDirection == Vector2.up) return Vector2.down;
            if (_pathMakerDirection == Vector2.left) return Vector2.right;
            if (_pathMakerDirection == Vector2.down) return Vector2.up;
            return Vector2.left;
        }
        
        #endregion
        
        #region Walls

        /// <summary>
        /// 벽을 생성합니다.
        /// </summary>
        private void GenerateWall()
        {
            // 모든 타일을 반복합니다.
            for (int x = 0; x < levelSize.x - 1; x++)
            {
                for (int y = 0; y < levelSize.y - 1; y++)
                {
                    // 현재 타일이 '바닥' 유형인 경우 주변 타일을 확인합니다.
                    if (tiles[x, y] == tileType.floor)
                    {
                        GenerateSurroundingWall(x, y);
                    }
                }
            }

            // 격리된 벽을 제거하는 메서드를 호출합니다.
            RemoveIsolatedWalls();

            // 설정에 따라 부자연스러운 벽을 제거하는 메서드를 호출합니다.
            if (removeUnnaturalWalls)
                RemoveUnnaturalWalls();
        }
        
        /// <summary>
        /// 주변 좌표를 확인하고 주변의 벽을 생성합니다.
        /// </summary>
        private void GenerateSurroundingWall(int x, int y)
        {
            // 확인할 방향을 정의합니다.
            List<(int, int)> directions = new List<(int, int)>
            {
                (1, 0), (1, 1), (0, 1), (-1, 1),
                (-1, 0), (-1, -1), (0, -1), (1, -1)
            };

            foreach ((int, int) direction in directions)
            {
                int newX = x + direction.Item1;
                int newY = y + direction.Item2;

                // 주어진 방향의 타일이 '비어 있는지' 확인하고 대각선 방향의 경우 모서리 벽이 생성되어야 하는지 확인합니다.
                if (IsWithinBounds(newX, newY) && tiles[newX, newY] == tileType.empty)
                {
                    bool isDiagonal = direction.Item1 != 0 && direction.Item2 != 0;
                    if (!isDiagonal || (isDiagonal && spawnCornerWalls))
                    {
                        tiles[newX, newY] = tileType.wall;
                    }
                }
            }//foreach ((int, int) direction in directions)
        }
        
        /// <summary>
        /// 주어진 좌표가 레벨 범위 내에 있는지 확인합니다.
        /// </summary>
        private bool IsWithinBounds(int x, int y)
        {
            return x >= 0 && x < levelSize.x && y >= 0 && y < levelSize.y;
        }
        
        
        /// <summary>
        /// 이 기능은 레벨에서 고립된 벽을 제거하는 데 사용됩니다.
        /// 격리된 벽은 4면(오른쪽, 위, 왼쪽, 아래)에서 '바닥' 타일로 둘러싸인 '벽' 타일로 정의됩니다.
        /// 함수는 레벨의 각 타일을 반복하고 각 '벽' 타일에 대해 주변 타일의 유형을 확인합니다.
        /// 모두 '바닥'인 경우 타일은 분리된 것으로 간주하고 유형을 '바닥'으로 변경합니다.
        /// </summary>
        private void RemoveIsolatedWalls()
        {
            for (int x = 0; x < levelSize.x - 1; x++)
            {
                for (int y = 0; y < levelSize.y - 1; y++)
                {
                    // 현재 타일이 '벽' 유형인지 확인합니다.
                    // 그런 다음 주변 타일(오른쪽, 위, 왼쪽, 아래)이 모두 '바닥' 유형인지 확인합니다.
                    // 이는 현재 '벽' 타일이 격리되어 있음을 의미합니다. 즉, 다른 '벽' 타일과 연결되지 않고 '바닥' 타일로 둘러싸여 있습니다.
                    if (tiles[x, y] == tileType.wall &&
                        tiles[x + 1, y] == tileType.floor && tiles[x, y + 1] == tileType.floor &&
                        tiles[x - 1, y] == tileType.floor && tiles[x, y - 1] == tileType.floor)
                    {
                        tiles[x, y] = tileType.floor;
                    }
                }
            }
        }
        
        /// <summary>
        /// 이 함수는 레벨에서 자연스럽지 않은 벽을 제거하는 데 사용됩니다.
        /// 자연스럽지 않은 벽은 '빈' 타일로 둘러싸인 '벽' 타일을 의미합니다.
        /// 함수는 레벨의 각 타일을 순회하며, 각 '벽' 타일에 대해 주변 타일의 유형을 확인합니다. 
        /// 모든 주변 타일이 '빈' 타일이 아닌 경우, 현재 '벽' 타일은 완전히 다른 타일('바닥' 또는 '벽')로 둘러싸인 것으로 간주되며, 그 타일의 유형이 '바닥'으로 변경됩니다.
        /// </summary>
        private void RemoveUnnaturalWalls()
        {
            // 주변 타일을 확인하면서. 배열의 가장자리는 '부자연스러운' 벽이 될 수 없으므로 검사하지 않습니다.
            for (int x = 1; x < levelSizeCut.x - 1; x++)
            {
                for (int y = 1; y < levelSizeCut.y - 1; y++)
                {
                    // 현재 타일이 '벽' 유형인지 확인합니다.
                    if (tiles[x, y] == tileType.wall)
                    {
                        // 그런 다음 모든 주변 타일(오른쪽, 오른쪽 위, 위, 왼쪽 위, 왼쪽, 왼쪽 아래, 아래, 오른쪽 아래)이 '비어 있지' 않은지 확인합니다.
                        // 이는 현재 '벽' 타일이 다른 타일('바닥' 또는 '벽')에 의해 완전히 둘러싸여 있음을 의미합니다.
                        if (tiles[x + 1, y] != tileType.empty && tiles[x + 1, y + 1] != tileType.empty &&
                            tiles[x, y + 1] != tileType.empty && tiles[x - 1, y + 1] != tileType.empty &&
                            tiles[x - 1, y] != tileType.empty && tiles[x - 1, y - 1] != tileType.empty &&
                            tiles[x, y - 1] != tileType.empty && tiles[x + 1, y - 1] != tileType.empty)
                        {
                            tiles[x, y] = tileType.floor;
                        }
                    }
                }
            }
        }

        #endregion


        #region Overlay

        public void InstantiateFloorOverlay()
        {
            for (int x = 0; x < levelSize.x - 1; x++)
            {
                for (int y = 0; y < levelSize.y - 1; y++)
                {
                    if (patternFloor == patternType.perlinNoise && IsFloorNotTouchingWall(x,y)) 
                        CreatePerlinFloor(x, y);
                    
                    else if (patternFloor == patternType.checker && IsFloorNotTouchingWall(x, y)) 
                        CreateCheckerFloor(x, y);
                    
                    else if (patternFloor == patternType.wideChecker && IsFloorNotTouchingWall(x, y)) 
                        CreateWideCheckerFloor(x, y);
                    
                    else if (patternFloor == patternType.lineLeft && IsFloorNotTouchingWall(x, y)) 
                        CreateLineLeftFloor(x, y);
                    
                    else if (patternFloor == patternType.lineRight && IsFloorNotTouchingWall(x, y)) 
                        CreateLineRightFloor(x, y);
                }
            }
        }


        public void InstanciateWallOverlay()
        {
            for (int x = 0; x < levelSize.x - 1; x++)
            {
                for (int y = 0; y < levelSize.y - 1; y++)
                {
                    if (patternWall == patternType.perlinNoise && tiles[x, y] == tileType.wall) CreatePerlinWall(x, y);
                    else if (patternWall == patternType.checker && tiles[x, y] == tileType.wall) CreateCheckerWall(x, y);
                    else if (patternWall == patternType.wideChecker && tiles[x, y] == tileType.wall) CreateWideCheckerWall(x, y);
                    else if (patternWall == patternType.lineLeft && tiles[x, y] == tileType.wall) CreateLineLeftWall(x, y);
                    else if (patternWall == patternType.lineRight && tiles[x, y] == tileType.wall) CreateLineRightWall(x, y);
                }
            }
        }


        /// <summary>
        /// Perlin 노이즈 기반 바닥 패턴을 만듭니다.
        /// 이 함수는 지도 바닥에 대한 패턴을 생성하기 위해 Perlin 노이즈를 활용합니다.
        /// Perlin 노이즈 값, noiseCutoffFloor 및 randomFloorOverlayChance 매개변수를 기반으로 각 타일에 패턴 또는 임의 타일 유형을 할당합니다.
        /// </summary>
        public void CreatePerlinFloor(int _posX, int _posY)
        {
            // Perlin 노이즈 함수를 사용하여 매끄럽고 연속적인 노이즈 값을 생성합니다. 이는 위치(_posX, _posY)에 해당 노이즈 스케일을 곱한 다음 임의 오프셋을 추가하여 수행됩니다.
            float value = Mathf.PerlinNoise(_posX * noiseScaleFloor.y + Random.Range(0f, 1f) * noiseScaleFloor.x,
                _posY * noiseScaleFloor.y + Random.Range(0f, 1f) * noiseScaleFloor.y);
            
            // 생성된 노이즈 값이 지정된 컷오프 값보다 크면 이를 '패턴'의 일부로 간주하고 (_posX, _posY)의 타일을 overlayTiles 배열의 floorPattern으로 표시합니다.
            if (value > noiseCutoffFloor)
            {
                overlayTiles[_posX, _posY] = overlayType.floorPattern;
            }
            // 노이즈 값이 컷오프를 초과하지 않고 무작위 바닥 오버레이 타일('drawFloorOverlayRandomTiles' 부울에 의해 제어됨)을 그릴 수 있고
            // 생성된 무작위 숫자가 정의된 무작위 오버레이 확률보다 작은 경우 타일을 (_posX, _posY)를 overlayTiles 배열의 임의 바닥 타일로 지정합니다.
            else if (drawFloorOverlayRandomTiles && Random.Range(0, 100) < randomFloorOverlayChance)
            {
                overlayTiles[_posX, _posY] = overlayType.floorRandom;
            }
        }


        public void CreatePerlinWall(int _posX, int _posY)
        {
            float value = Mathf.PerlinNoise(_posX * noiseScaleWall.y + Random.Range(0f, 1f) * noiseScaleWall.x, _posY * noiseScaleWall.y + Random.Range(0f, 1f) * noiseScaleWall.y);

            if (value > noiseCutoffWall && drawWallOverlayPatternTiles) overlayTiles[_posX, _posY] = overlayType.wallPattern;
            else if (drawWallOverlayRandomTiles && Random.Range(0, 100) < randomWallOverlayChance) overlayTiles[_posX, _posY] = overlayType.wallRandom;
        }


        /// <summary>
        /// Perlin 노이즈와 타일 위치에 대한 모드 기능을 사용하여 바닥에 체크 무늬를 만듭니다.
        /// 이 함수는 지도 바닥에 체크 무늬를 생성합니다. Perlin 노이즈 값과 타일의 x 및 y 위치의 합에 따라 패턴 또는 임의의 타일 유형을 각 타일에 할당합니다.
        /// </summary>
        public void CreateCheckerFloor(int _posX, int _posY)
        {
            // 보다 자연스러운 랜덤 패턴을 만드는 데 사용되는 Perlin 노이즈 알고리즘을 사용하여 값을 생성합니다.
            float value = Mathf.PerlinNoise(_posX * noiseScaleFloor.y + Random.Range(0f, 1f) * noiseScaleFloor.x,
                _posY * noiseScaleFloor.y + Random.Range(0f, 1f) * noiseScaleFloor.y);

            // 생성된 Perlin 노이즈 값이 특정 임계값(noiseCutoffFloor)보다 크고
            // _posX와 _posY의 합이 짝수이면 overlayTiles의 현재 위치에 'floorPattern'을 할당합니다.
            // (_posX + _posY)의 균일성 검사는 체커 패턴을 생성하는 것입니다. 합이 짝수이면 하나의 '색상'이고, 홀수이면 다른 색입니다.
            if (value > noiseCutoffFloor && (_posX + _posY) % 2 == 0)
            {
                overlayTiles[_posX, _posY] = overlayType.floorPattern;
            }
            // drawFloorOverlayRandomTiles가 true이고 0에서 100 사이의 임의의 숫자가 randomFloorOverlayChance보다 작은 경우
            // overlayTiles의 현재 위치에 'floorRandom'을 할당합니다.
            // 이것은 바닥 패턴에 임의의 변형을 추가하는 데 사용됩니다.
            else if (drawFloorOverlayRandomTiles && Random.Range(0, 100) < randomFloorOverlayChance)
            {
                overlayTiles[_posX, _posY] = overlayType.floorRandom;
            }
        }

        public void CreateCheckerWall(int _posX, int _posY)
        {
            float value = Mathf.PerlinNoise(_posX * noiseScaleWall.y + Random.Range(0f, 1f) * noiseScaleWall.x, _posY * noiseScaleWall.y + Random.Range(0f, 1f) * noiseScaleWall.y);

            if (value > noiseCutoffWall && drawWallOverlayPatternTiles && (_posX + _posY) % 2 == 0) overlayTiles[_posX, _posY] = overlayType.wallPattern;
            else if (drawWallOverlayRandomTiles && Random.Range(0, 100) < randomWallOverlayChance) overlayTiles[_posX, _posY] = overlayType.wallRandom;
        }

        /// <summary>
        /// 타일 위치에 대한 Perlin 노이즈 및 모듈러스 연산을 사용하여 바닥에 넓은 체커 패턴을 만듭니다.
        /// </summary>
        public void CreateWideCheckerFloor(int _posX, int _posY)
        {
            float value = Mathf.PerlinNoise(_posX * noiseScaleFloor.y + Random.Range(0f, 1f) * noiseScaleFloor.x,
                _posY * noiseScaleFloor.y + Random.Range(0f, 1f) * noiseScaleFloor.y);

            if (value > noiseCutoffFloor && (_posX + _posY) % 4 == 0 && _posX % 2 == 0 && _posY % 2 == 0)
            {
                overlayTiles[_posX, _posY] = overlayType.floorPattern;
            }
            else if (drawFloorOverlayRandomTiles && Random.Range(0, 100) < randomFloorOverlayChance)
            {
                overlayTiles[_posX, _posY] = overlayType.floorRandom;
            }
        }


        public void CreateWideCheckerWall(int _posX, int _posY)
        {
            float value = Mathf.PerlinNoise(_posX * noiseScaleWall.y + Random.Range(0f, 1f) * noiseScaleWall.x, _posY * noiseScaleWall.y + Random.Range(0f, 1f) * noiseScaleWall.y);

            if (value > noiseCutoffWall && drawWallOverlayPatternTiles && (_posX + _posY) % 4 == 0 && _posX % 2 == 0 && _posY % 2 == 0) overlayTiles[_posX, _posY] = overlayType.wallPattern;
            else if (drawWallOverlayRandomTiles && Random.Range(0, 100) < randomWallOverlayChance) overlayTiles[_posX, _posY] = overlayType.wallRandom;
        }


        public void CreateLineLeftFloor(int _posX, int _posY)
        {
            float value = Mathf.PerlinNoise(_posX * noiseScaleFloor.y + Random.Range(0f, 1f) * noiseScaleFloor.x, _posY * noiseScaleFloor.y + Random.Range(0f, 1f) * noiseScaleFloor.y);

            if (value > noiseCutoffFloor && (-_posX + _posY) % 3 == 0) overlayTiles[_posX, _posY] = overlayType.floorPattern;
            else if (drawFloorOverlayRandomTiles && Random.Range(0, 100) < randomFloorOverlayChance) overlayTiles[_posX, _posY] = overlayType.floorRandom;
        }


        public void CreateLineLeftWall(int _posX, int _posY)
        {
            float value = Mathf.PerlinNoise(_posX * noiseScaleWall.y + Random.Range(0f, 1f) * noiseScaleWall.x, _posY * noiseScaleWall.y + Random.Range(0f, 1f) * noiseScaleWall.y);

            if (value > noiseCutoffWall && drawWallOverlayPatternTiles && (-_posX + _posY) % 3 == 0) overlayTiles[_posX, _posY] = overlayType.wallPattern;
            else if (drawWallOverlayRandomTiles && Random.Range(0, 100) < randomWallOverlayChance) overlayTiles[_posX, _posY] = overlayType.wallRandom;
        }


        public void CreateLineRightFloor(int _posX, int _posY)
        {
            float value = Mathf.PerlinNoise(_posX * noiseScaleFloor.y + Random.Range(0f, 1f) * noiseScaleFloor.x, _posY * noiseScaleFloor.y + Random.Range(0f, 1f) * noiseScaleFloor.y);

            if (value > noiseCutoffFloor && (_posX + _posY) % 3 == 0) overlayTiles[_posX, _posY] = overlayType.floorPattern;
            else if (drawFloorOverlayRandomTiles && Random.Range(0, 100) < randomFloorOverlayChance) overlayTiles[_posX, _posY] = overlayType.floorRandom;
        }


        public void CreateLineRightWall(int _posX, int _posY)
        {
            float value = Mathf.PerlinNoise(_posX * noiseScaleWall.y + Random.Range(0f, 1f) * noiseScaleWall.x, _posY * noiseScaleWall.y + Random.Range(0f, 1f) * noiseScaleWall.y);

            if (value > noiseCutoffWall && drawWallOverlayPatternTiles && (_posX + _posY) % 3 == 0) overlayTiles[_posX, _posY] = overlayType.wallPattern;
            else if (drawWallOverlayRandomTiles && Random.Range(0, 100) < randomWallOverlayChance) overlayTiles[_posX, _posY] = overlayType.wallRandom;
        }
        
        /// <summary>
        /// 이 함수는 레벨의 모든 타일을 순회하며, 일정 확률에 따라 'floorRandom' 오버레이를 바닥 타일에 추가하는 작업을 수행합니다.
        /// 확률은 'randomFloorOverlayChance' 변수로 정의되며, 0과 100 사이의 값을 가집니다.
        /// 이 값이 크면 클수록, 더 많은 바닥 타일에 무작위 오버레이가 적용됩니다.
        /// </summary>
        public void InstantiateFloorRandomOverlay()
        {
            for (int x = 0; x < levelSize.x - 1; x++)
            {
                for (int y = 0; y < levelSize.y - 1; y++)
                {
                    if (Random.Range(0, 100) < randomFloorOverlayChance && tiles[x, y] == tileType.floor)
                    {
                        overlayTiles[x, y] = overlayType.floorRandom;
                    }
                }
            }
        }
        
        /// <summary>
        /// 이 함수는 레벨의 모든 타일을 순회하며, 일정 확률에 따라 'wallRandom' 오버레이를 벽 타일에 추가하는 작업을 수행합니다.
        /// 이 값이 크면 클수록, 더 많은 벽 타일에 무작위 오버레이가 적용됩니다.
        /// </summary>
        public void InstantiateWallRandomOverlay()
        {
            for (int x = 0; x < levelSize.x - 1; x++)
            {
                for (int y = 0; y < levelSize.y - 1; y++)
                {
                    if (Random.Range(0, 100) < randomWallOverlayChance && tiles[x, y] == tileType.wall)
                    {
                        overlayTiles[x, y] = overlayType.wallRandom;
                    }
                }
            }
        }

        #endregion


        #region Spawn 

        private void Spawn()
        {
            if (generation == genType.generateTile)
            {
                if (drawTilesOrientation) SpawnGridTilesOriented();
                else SpawnGridTiles();
            }
            else
            {
                if (drawTilesOrientation)
                {
                    if (fillAllTiles) SpawnAllTilesOriented();
                    else SpawnTilesOriented();
                }
                else SpawnTiles();

                RotateLevel();
            }
        }


        private void SpawnGridTilesOriented()
        {
            //references
            Tilemap emptyMap = new Tilemap();
            Tilemap overlayMap = new Tilemap();
            Tilemap floorMap = floorParent.GetComponent<Tilemap>();
            Tilemap wallMap = wallParent.GetComponent<Tilemap>();

            if (drawFloorOverlayPatternTiles || drawFloorOverlayRandomTiles || drawWallOverlayPatternTiles || drawWallOverlayRandomTiles) overlayMap = overlayParent.GetComponent<Tilemap>();
            if (drawEmptyTiles) emptyMap = emptyParent.GetComponent<Tilemap>();


            //instanciate
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    if (tiles[x, y] == tileType.floor)
                    {
                        bool wallTop = tiles[x, y + 1] == tileType.wall;
                        bool wallLeft = tiles[x - 1, y] == tileType.wall;
                        bool wallBottom = tiles[x, y - 1] == tileType.wall;
                        bool wallRight = tiles[x + 1, y] == tileType.wall;

                        bool wallTopLeft = tiles[x - 1, y + 1] == tileType.wall;
                        bool wallTopRight = tiles[x + 1, y + 1] == tileType.wall;
                        bool wallBottomLeft = tiles[x - 1, y - 1] == tileType.wall;
                        bool wallBottomRight = tiles[x + 1, y - 1] == tileType.wall;


                        //4 sides
                        if (!wallTop && !wallLeft && !wallBottom && !wallRight)
                        {
                            if(drawCorners)
                            {
                                //one side
                                if (wallTopLeft && !wallTopRight && !wallBottomLeft && !wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C1);
                                else if (!wallTopLeft && !wallTopRight && wallBottomLeft && !wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C2);
                                else if (!wallTopLeft && !wallTopRight && !wallBottomLeft && wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C3);
                                else if (!wallTopLeft && wallTopRight && !wallBottomLeft && !wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C4);

                                //2 sides
                                else if (wallTopLeft && wallTopRight && !wallBottomLeft && !wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C5);
                                else if (wallTopLeft && !wallTopRight && wallBottomLeft && !wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C6);
                                else if (!wallTopLeft && !wallTopRight && wallBottomLeft && wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C7);
                                else if (!wallTopLeft && wallTopRight && !wallBottomLeft && wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C8);
                                else if (!wallTopLeft && wallTopRight && wallBottomLeft && !wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C9);
                                else if (wallTopLeft && !wallTopRight && !wallBottomLeft && wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C10);

                                //3 sides
                                else if (wallTopLeft && wallTopRight && wallBottomLeft && !wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C11);
                                else if (wallTopLeft && !wallTopRight && wallBottomLeft && wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C12);
                                else if (!wallTopLeft && wallTopRight && wallBottomLeft && wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C13);
                                else if (wallTopLeft && wallTopRight && !wallBottomLeft && wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C14);
                                else if (wallTopLeft && wallTopRight && wallBottomLeft && wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C31);
                                else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_1);
                            }
                            else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_1);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }

                        //one side
                        else if (wallTop && !wallLeft && !wallBottom && !wallRight)
                        {
                            if (drawCorners)
                            {
                                if (wallBottomLeft && wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C17);
                                else if (wallBottomLeft) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C15);
                                else if (wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C16);
                                else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_2);
                            }
                            else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_2);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (!wallTop && wallLeft && !wallBottom && !wallRight)
                        {
                            if (drawCorners)
                            {
                                if (wallBottomRight && wallTopRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C20);
                                else if (wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C18);
                                else if (wallTopRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C19);
                                else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_3);
                            }
                            else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_3);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (!wallTop && !wallLeft && wallBottom && !wallRight)
                        {
                            if (drawCorners)
                            {
                                if (wallTopLeft && wallTopRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C23);
                                else if (wallTopLeft) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C21);
                                else if (wallTopRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C22);
                                else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_4);
                            }
                            else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_4);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (!wallTop && !wallLeft && !wallBottom && wallRight)
                        {
                            if (drawCorners)
                            {
                                if (wallTopLeft && wallBottomLeft) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C26);
                                else if (wallTopLeft) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C24);
                                else if (wallBottomLeft) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C25);
                                else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_5);
                            }
                            else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_5);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }

                        //2 sides
                        else if (wallTop && wallLeft && !wallBottom && !wallRight)
                        {
                            if (drawCorners && wallBottomRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C27);
                            else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_6);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (!wallTop && wallLeft && wallBottom && !wallRight)
                        {
                            if (drawCorners && wallTopRight) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C28);
                            else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_7);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (!wallTop && !wallLeft && wallBottom && wallRight)
                        {
                            if (drawCorners && wallTopLeft) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C29);
                            else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_8);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (wallTop && !wallLeft && !wallBottom && wallRight)
                        {
                            if (drawCorners && wallBottomLeft) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_C30);
                            else floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_9);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (!wallTop && wallLeft && !wallBottom && wallRight)
                        {
                            floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_10);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (wallTop && !wallLeft && wallBottom && !wallRight)
                        {
                            floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_11);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }

                        //3 sides
                        else if (wallTop && wallLeft && !wallBottom && wallRight)
                        {
                            floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_12);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (wallTop && wallLeft && wallBottom && !wallRight)
                        {
                            floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_13);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (!wallTop && wallLeft && wallBottom && wallRight)
                        {
                            floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_14);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                        else if (wallTop && !wallLeft && wallBottom && wallRight)
                        {
                            floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_15);

                            if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                        }
                    }

                    else if (tiles[x, y] == tileType.wall)
                    {
                        bool floorTop = false;
                        bool floorLeft = false;
                        bool floorBottom = false;
                        bool floorRight = false;

                        bool floorTopLeft = false;
                        bool floorTopRight = false;
                        bool floorBottomLeft = false;
                        bool floorBottomRight = false;

                        if (y + 1 < levelSize.y) floorTop = tiles[x, y + 1] == tileType.floor;
                        if (x - 1 > 0) floorLeft = tiles[x - 1, y] == tileType.floor;
                        if (y - 1 > 0) floorBottom = tiles[x, y - 1] == tileType.floor;
                        if (x + 1 < levelSize.x) floorRight = tiles[x + 1, y] == tileType.floor;

                        if (x - 1 > 0 && y + 1 < levelSize.y) floorTopLeft = tiles[x - 1, y + 1] == tileType.floor;
                        if (x + 1 < levelSize.x && y + 1 < levelSize.y) floorTopRight = tiles[x + 1, y + 1] == tileType.floor;
                        if (x - 1 > 0 && y - 1 > 0) floorBottomLeft = tiles[x - 1, y - 1] == tileType.floor;
                        if (x + 1 < levelSize.x && y - 1 > 0) floorBottomRight = tiles[x + 1, y - 1] == tileType.floor;


                        //one side
                        if (floorTop && !floorLeft && !floorBottom && !floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorBottomLeft && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C17);
                                else if (floorBottomLeft) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C15);
                                else if (floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C16);
                                else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_2);
                            }
                            else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_2);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_2);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_2);
                        }
                        else if (!floorTop && floorLeft && !floorBottom && !floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorTopRight && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C20);
                                else if (floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C18);
                                else if (floorTopRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C19);
                                else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_3);
                            }
                            else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_3);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_3);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_3);
                        }
                        else if (!floorTop && !floorLeft && floorBottom && !floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorTopRight && floorTopLeft) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C23);
                                else if (floorTopLeft) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C21);
                                else if (floorTopRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C22);
                                else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_4);
                            }
                            else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_4);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_4);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_4);
                        }
                        else if (!floorTop && !floorLeft && !floorBottom && floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorBottomLeft && floorTopLeft) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C26);
                                else if (floorTopLeft) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C24);
                                else if (floorBottomLeft) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C25);
                                else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_5);
                            }
                            else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_5);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_5);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_5);
                        }

                        //2 sides
                        else if (floorTop && floorLeft && !floorBottom && !floorRight)
                        {
                            if (drawCorners && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C27);
                            else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_6);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_6);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_6);
                        }
                        else if (!floorTop && floorLeft && floorBottom && !floorRight)
                        {
                            if (drawCorners && floorTopRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C28);
                            else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_7);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_7);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_7);
                        }
                        else if (!floorTop && !floorLeft && floorBottom && floorRight)
                        {
                            if (drawCorners && floorTopLeft) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C29);
                            else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_8);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_8);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_8);
                        }
                        else if (floorTop && !floorLeft && !floorBottom && floorRight)
                        {
                            if (drawCorners && floorBottomLeft) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C30);
                            else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_9);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_9);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_9);
                        }
                        else if (!floorTop && floorLeft && !floorBottom && floorRight)
                        {
                            wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_10);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_10);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_10);
                        }
                        else if (floorTop && !floorLeft && floorBottom && !floorRight)
                        {
                            wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_11);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_11);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_11);
                        }

                        //3 sides
                        else if (floorTop && floorLeft && !floorBottom && floorRight)
                        {
                            wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_12);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_12);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_12);
                        }
                        else if (floorTop && floorLeft && floorBottom && !floorRight)
                        {
                            wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_13);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_13);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_13);
                        }
                        else if (!floorTop && floorLeft && floorBottom && floorRight)
                        {
                            wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_14);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_14);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_14);
                        }
                        else if (floorTop && !floorLeft && floorBottom && floorRight)
                        {
                            wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_15);

                            if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_15);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_15);
                        }

                        //corner walls
                        else if (spawnCornerWalls)
                        {
                            if (drawCorners)
                            {
                                //one side
                                if (floorTopLeft && !floorTopRight && !floorBottomLeft && !floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C1);
                                else if (!floorTopLeft && !floorTopRight && floorBottomLeft && !floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C2);
                                else if (!floorTopLeft && !floorTopRight && !floorBottomLeft && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C3);
                                else if (!floorTopLeft && floorTopRight && !floorBottomLeft && !floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C4);

                                //2 sides
                                else if (floorTopLeft && floorTopRight && !floorBottomLeft && !floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C5);
                                else if (floorTopLeft && !floorTopRight && floorBottomLeft && !floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C6);
                                else if (!floorTopLeft && !floorTopRight && floorBottomLeft && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C7);
                                else if (!floorTopLeft && floorTopRight && !floorBottomLeft && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C8);
                                else if (!floorTopLeft && floorTopRight && floorBottomLeft && !floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C9);
                                else if (floorTopLeft && !floorTopRight && !floorBottomLeft && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C10);

                                //3 sides
                                else if (floorTopLeft && floorTopRight && floorBottomLeft && !floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C11);
                                else if (floorTopLeft && !floorTopRight && floorBottomLeft && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C12);
                                else if (!floorTopLeft && floorTopRight && floorBottomLeft && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C13);
                                else if (floorTopLeft && floorTopRight && !floorBottomLeft && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C14);
                                else if (floorTopLeft && floorTopRight && floorBottomLeft && floorBottomRight) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_C31);
                                else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_1);
                            }
                            else wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_1);
                        }
                        else if (drawEmptyTiles) emptyMap.SetTile(new Vector3Int(x, y, 0), emptyTile);
                    }

                    else if (drawEmptyTiles) emptyMap.SetTile(new Vector3Int(x, y, 0), emptyTile);
                }
            }
        }


        private void SpawnGridTiles()
        {
            //references
            Tilemap emptyMap = new Tilemap();
            Tilemap overlayMap = new Tilemap();
            Tilemap floorMap = floorParent.GetComponent<Tilemap>();
            Tilemap wallMap = wallParent.GetComponent<Tilemap>();

            if (drawFloorOverlayPatternTiles || drawFloorOverlayRandomTiles || drawWallOverlayPatternTiles || drawWallOverlayRandomTiles) overlayMap = overlayParent.GetComponent<Tilemap>();
            if (drawEmptyTiles) emptyMap = emptyParent.GetComponent<Tilemap>();


            //instanciate
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    if (tiles[x, y] == tileType.floor) floorMap.SetTile(new Vector3Int(x, y, 0), floorTile_1);
                    else if (tiles[x, y] == tileType.wall) wallMap.SetTile(new Vector3Int(x, y, 0), wallTile_1);
                    else if (drawEmptyTiles) emptyMap.SetTile(new Vector3Int(x, y, 0), emptyTile);

                    if (overlayTiles[x, y] == overlayType.floorPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternFloorTile);
                    else if (overlayTiles[x, y] == overlayType.floorRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomFloorTile);
                    else if (overlayTiles[x, y] == overlayType.wallPattern) overlayMap.SetTile(new Vector3Int(x, y, 0), patternWallTile_1);
                    else if (overlayTiles[x, y] == overlayType.wallRandom) overlayMap.SetTile(new Vector3Int(x, y, 0), randomWallTile_1);
                }
            }
        }


        private void SpawnAllTilesOriented()
        {
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    if (tiles[x, y] == tileType.floor)
                    {
                        bool wallTop = tiles[x, y + 1] == tileType.wall;
                        bool wallLeft = tiles[x - 1, y] == tileType.wall;
                        bool wallBottom = tiles[x, y - 1] == tileType.wall;
                        bool wallRight = tiles[x + 1, y] == tileType.wall;

                        bool wallTopLeft = tiles[x - 1, y + 1] == tileType.wall;
                        bool wallTopRight = tiles[x + 1, y + 1] == tileType.wall;
                        bool wallBottomLeft = tiles[x - 1, y - 1] == tileType.wall;
                        bool wallBottomRight = tiles[x + 1, y - 1] == tileType.wall;


                        //4 sides
                        if (!wallTop && !wallLeft && !wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners)
                                {
                                    //one side
                                    if (wallTopLeft && !wallTopRight && !wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C1, floorParent.transform, x, y);
                                    else if (!wallTopLeft && !wallTopRight && wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C2, floorParent.transform, x, y);
                                    else if (!wallTopLeft && !wallTopRight && !wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C3, floorParent.transform, x, y);
                                    else if (!wallTopLeft && wallTopRight && !wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C4, floorParent.transform, x, y);

                                    //2 sides
                                    else if (wallTopLeft && wallTopRight && !wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C5, floorParent.transform, x, y);
                                    else if (wallTopLeft && !wallTopRight && wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C6, floorParent.transform, x, y);
                                    else if (!wallTopLeft && !wallTopRight && wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C7, floorParent.transform, x, y);
                                    else if (!wallTopLeft && wallTopRight && !wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C8, floorParent.transform, x, y);
                                    else if (!wallTopLeft && wallTopRight && wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C9, floorParent.transform, x, y);
                                    else if (wallTopLeft && !wallTopRight && !wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C10, floorParent.transform, x, y);

                                    //3 sides
                                    else if (wallTopLeft && wallTopRight && wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C11, floorParent.transform, x, y);
                                    else if (wallTopLeft && !wallTopRight && wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C12, floorParent.transform, x, y);
                                    else if (!wallTopLeft && wallTopRight && wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C13, floorParent.transform, x, y);
                                    else if (wallTopLeft && wallTopRight && !wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C14, floorParent.transform, x, y);
                                    else if (wallTopLeft && wallTopRight && wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C31, floorParent.transform, x, y);
                                    else SpawnTile(floorTileObj_1, floorParent.transform, x, y);
                                }
                                else SpawnTile(floorTileObj_1, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }

                        //one side
                        else if (wallTop && !wallLeft && !wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners)
                                {
                                    if (wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C17, floorParent.transform, x, y);
                                    else if (wallBottomLeft) SpawnTile(floorTileObj_C15, floorParent.transform, x, y);
                                    else if (wallBottomRight) SpawnTile(floorTileObj_C16, floorParent.transform, x, y);
                                    else SpawnTile(floorTileObj_2, floorParent.transform, x, y);
                                }
                                else SpawnTile(floorTileObj_2, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && wallLeft && !wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners)
                                {
                                    if (wallBottomRight && wallTopRight) SpawnTile(floorTileObj_C20, floorParent.transform, x, y);
                                    else if (wallBottomRight) SpawnTile(floorTileObj_C18, floorParent.transform, x, y);
                                    else if (wallTopRight) SpawnTile(floorTileObj_C19, floorParent.transform, x, y);
                                    else SpawnTile(floorTileObj_3, floorParent.transform, x, y);
                                }
                                else SpawnTile(floorTileObj_3, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && !wallLeft && wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners)
                                {
                                    if (wallTopLeft && wallTopRight) SpawnTile(floorTileObj_C23, floorParent.transform, x, y);
                                    else if (wallTopLeft) SpawnTile(floorTileObj_C21, floorParent.transform, x, y);
                                    else if (wallTopRight) SpawnTile(floorTileObj_C22, floorParent.transform, x, y);
                                    else SpawnTile(floorTileObj_4, floorParent.transform, x, y);
                                }
                                else SpawnTile(floorTileObj_4, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && !wallLeft && !wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners)
                                {
                                    if (wallTopLeft && wallBottomLeft) SpawnTile(floorTileObj_C26, floorParent.transform, x, y);
                                    else if (wallTopLeft) SpawnTile(floorTileObj_C24, floorParent.transform, x, y);
                                    else if (wallBottomLeft) SpawnTile(floorTileObj_C25, floorParent.transform, x, y);
                                    else SpawnTile(floorTileObj_5, floorParent.transform, x, y);
                                }
                                else SpawnTile(floorTileObj_5, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }

                        //2 sides
                        else if (wallTop && wallLeft && !wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if(drawCorners && wallBottomRight) SpawnTile(floorTileObj_C27, floorParent.transform, x, y);
                                else SpawnTile(floorTileObj_6, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && wallLeft && wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners && wallTopRight) SpawnTile(floorTileObj_C28, floorParent.transform, x, y);
                                else SpawnTile(floorTileObj_7, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && !wallLeft && wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners && wallTopLeft) SpawnTile(floorTileObj_C29, floorParent.transform, x, y);
                                else SpawnTile(floorTileObj_8, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (wallTop && !wallLeft && !wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners && wallBottomLeft) SpawnTile(floorTileObj_C30, floorParent.transform, x, y);
                                else SpawnTile(floorTileObj_9, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && wallLeft && !wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTile(floorTileObj_10, floorParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (wallTop && !wallLeft && wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTile(floorTileObj_11, floorParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }

                        //3 sides
                        else if (wallTop && wallLeft && !wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTile(floorTileObj_12, floorParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (wallTop && wallLeft && wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTile(floorTileObj_13, floorParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && wallLeft && wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTile(floorTileObj_14, floorParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (wallTop && !wallLeft && wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTile(floorTileObj_15, floorParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                    }

                    else if (tiles[x, y] == tileType.wall)
                    {
                        bool floorTop = false;
                        bool floorLeft = false;
                        bool floorBottom = false;
                        bool floorRight = false;

                        bool floorTopLeft = false;
                        bool floorTopRight = false;
                        bool floorBottomLeft = false;
                        bool floorBottomRight = false;

                        if (y + 1 < levelSize.y) floorTop = tiles[x, y + 1] == tileType.floor;
                        if (x - 1 > 0) floorLeft = tiles[x - 1, y] == tileType.floor;
                        if (y - 1 > 0) floorBottom = tiles[x, y - 1] == tileType.floor;
                        if (x + 1 < levelSize.x) floorRight = tiles[x + 1, y] == tileType.floor;

                        if (x - 1 > 0 && y + 1 < levelSize.y) floorTopLeft = tiles[x - 1, y + 1] == tileType.floor;
                        if (x + 1 < levelSize.x && y + 1 < levelSize.y) floorTopRight = tiles[x + 1, y + 1] == tileType.floor;
                        if (x - 1 > 0 && y - 1 > 0) floorBottomLeft = tiles[x - 1, y - 1] == tileType.floor;
                        if (x + 1 < levelSize.x && y - 1 > 0) floorBottomRight = tiles[x + 1, y - 1] == tileType.floor;


                        //one side
                        if (floorTop && !floorLeft && !floorBottom && !floorRight)
                        {
                            if(drawCorners)
                            {
                                if (floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C17, wallParent.transform, x, y);
                                else if (floorBottomLeft) SpawnTile(wallTileObj_C15, wallParent.transform, x, y);
                                else if (floorBottomRight) SpawnTile(wallTileObj_C16, wallParent.transform, x, y);
                                else SpawnTile(wallTileObj_2, wallParent.transform, x, y);
                            }
                            else SpawnTile(wallTileObj_2, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_2, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_2, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && floorLeft && !floorBottom && !floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorTopRight && floorBottomRight) SpawnTile(wallTileObj_C20, wallParent.transform, x, y);
                                else if (floorBottomRight) SpawnTile(wallTileObj_C18, wallParent.transform, x, y);
                                else if (floorTopRight) SpawnTile(wallTileObj_C19, wallParent.transform, x, y);
                                else SpawnTile(wallTileObj_3, wallParent.transform, x, y);
                            }
                            else SpawnTile(wallTileObj_3, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_3, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_3, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && !floorLeft && floorBottom && !floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorTopRight && floorTopLeft) SpawnTile(wallTileObj_C23, wallParent.transform, x, y);
                                else if (floorTopLeft) SpawnTile(wallTileObj_C21, wallParent.transform, x, y);
                                else if (floorTopRight) SpawnTile(wallTileObj_C22, wallParent.transform, x, y);
                                else SpawnTile(wallTileObj_4, wallParent.transform, x, y);
                            }
                            else SpawnTile(wallTileObj_4, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_4, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_4, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && !floorLeft && !floorBottom && floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorBottomLeft && floorTopLeft) SpawnTile(wallTileObj_C26, wallParent.transform, x, y);
                                else if (floorTopLeft) SpawnTile(wallTileObj_C24, wallParent.transform, x, y);
                                else if (floorBottomLeft) SpawnTile(wallTileObj_C25, wallParent.transform, x, y);
                                else SpawnTile(wallTileObj_5, wallParent.transform, x, y);
                            }
                            else SpawnTile(wallTileObj_5, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_5, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_5, overlayParent.transform, x, y);
                        }

                        //2 sides
                        else if (floorTop && floorLeft && !floorBottom && !floorRight)
                        {
                            if(drawCorners && floorBottomRight) SpawnTile(wallTileObj_C27, wallParent.transform, x, y);
                            else SpawnTile(wallTileObj_6, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_6, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_6, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && floorLeft && floorBottom && !floorRight)
                        {
                            if (drawCorners && floorTopRight) SpawnTile(wallTileObj_C28, wallParent.transform, x, y);
                            else SpawnTile(wallTileObj_7, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_7, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_7, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && !floorLeft && floorBottom && floorRight)
                        {
                            if (drawCorners && floorTopLeft) SpawnTile(wallTileObj_C29, wallParent.transform, x, y);
                            else SpawnTile(wallTileObj_8, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_8, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_8, overlayParent.transform, x, y);
                        }
                        else if (floorTop && !floorLeft && !floorBottom && floorRight)
                        {
                            if (drawCorners && floorBottomLeft) SpawnTile(wallTileObj_C30, wallParent.transform, x, y);
                            else SpawnTile(wallTileObj_9, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_9, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_9, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && floorLeft && !floorBottom && floorRight)
                        {
                            SpawnTile(wallTileObj_10, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_10, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_10, overlayParent.transform, x, y);
                        }
                        else if (floorTop && !floorLeft && floorBottom && !floorRight)
                        {
                            SpawnTile(wallTileObj_11, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_11, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_11, overlayParent.transform, x, y);
                        }

                        //3 sides
                        else if (floorTop && floorLeft && !floorBottom && floorRight)
                        {
                            SpawnTile(wallTileObj_12, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_12, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_12, overlayParent.transform, x, y);
                        }
                        else if (floorTop && floorLeft && floorBottom && !floorRight)
                        {
                            SpawnTile(wallTileObj_13, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_13, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_13, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && floorLeft && floorBottom && floorRight)
                        {
                            SpawnTile(wallTileObj_14, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_14, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_14, overlayParent.transform, x, y);
                        }
                        else if (floorTop && !floorLeft && floorBottom && floorRight)
                        {
                            SpawnTile(wallTileObj_15, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_15, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_15, overlayParent.transform, x, y);
                        }

                        //corner walls
                        else if (spawnCornerWalls)
                        {
                            if (drawCorners)
                            {
                                //one side
                                if (floorTopLeft && !floorTopRight && !floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C1, wallParent.transform, x, y);
                                else if (!floorTopLeft && !floorTopRight && floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C2, wallParent.transform, x, y);
                                else if (!floorTopLeft && !floorTopRight && !floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C3, wallParent.transform, x, y);
                                else if (!floorTopLeft && floorTopRight && !floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C4, wallParent.transform, x, y);

                                //2 sides
                                else if (floorTopLeft && floorTopRight && !floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C5, wallParent.transform, x, y);
                                else if (floorTopLeft && !floorTopRight && floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C6, wallParent.transform, x, y);
                                else if (!floorTopLeft && !floorTopRight && floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C7, wallParent.transform, x, y);
                                else if (!floorTopLeft && floorTopRight && !floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C8, wallParent.transform, x, y);
                                else if (!floorTopLeft && floorTopRight && floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C9, wallParent.transform, x, y);
                                else if (floorTopLeft && !floorTopRight && !floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C10, wallParent.transform, x, y);

                                //3 sides
                                else if (floorTopLeft && floorTopRight && floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C11, wallParent.transform, x, y);
                                else if (floorTopLeft && !floorTopRight && floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C12, wallParent.transform, x, y);
                                else if (!floorTopLeft && floorTopRight && floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C13, wallParent.transform, x, y);
                                else if (floorTopLeft && floorTopRight && !floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C14, wallParent.transform, x, y);
                                else if (floorTopLeft && floorTopRight && floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C31, wallParent.transform, x, y);
                                else SpawnTile(wallTileObj_1, wallParent.transform, x, y);
                            }
                            else SpawnTile(wallTileObj_1, wallParent.transform, x, y);
                        }
                        else if (drawEmptyTiles) SpawnTile(emptyTileObj, emptyParent.transform, x, y);
                    }

                    else if (drawEmptyTiles) SpawnTile(emptyTileObj, emptyParent.transform, x, y);
                }
            }
        }


        private void SpawnTilesOriented()
        {
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    if (tiles[x, y] == tileType.floor)
                    {
                        bool wallTop = tiles[x, y + 1] == tileType.wall;
                        bool wallLeft = tiles[x - 1, y] == tileType.wall;
                        bool wallBottom = tiles[x, y - 1] == tileType.wall;
                        bool wallRight = tiles[x + 1, y] == tileType.wall;

                        bool wallTopLeft = tiles[x - 1, y + 1] == tileType.wall;
                        bool wallTopRight = tiles[x + 1, y + 1] == tileType.wall;
                        bool wallBottomLeft = tiles[x - 1, y - 1] == tileType.wall;
                        bool wallBottomRight = tiles[x + 1, y - 1] == tileType.wall;


                        //4 sides
                        if (!wallTop && !wallLeft && !wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners)
                                {
                                    //one side
                                    if (wallTopLeft && !wallTopRight && !wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C1, floorParent.transform, x, y);
                                    else if (!wallTopLeft && !wallTopRight && wallBottomLeft && !wallBottomRight) SpawnTileRotated(floorTileObj_C1, floorParent.transform, -90f, x, y);
                                    else if (!wallTopLeft && !wallTopRight && !wallBottomLeft && wallBottomRight) SpawnTileRotated(floorTileObj_C1, floorParent.transform, 180f, x, y);
                                    else if (!wallTopLeft && wallTopRight && !wallBottomLeft && !wallBottomRight) SpawnTileRotated(floorTileObj_C1, floorParent.transform, 90f, x, y);

                                    //2 sides
                                    else if (wallTopLeft && wallTopRight && !wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C5, floorParent.transform, x, y);
                                    else if (wallTopLeft && !wallTopRight && wallBottomLeft && !wallBottomRight) SpawnTileRotated(floorTileObj_C5, floorParent.transform, -90f, x, y);
                                    else if (!wallTopLeft && !wallTopRight && wallBottomLeft && wallBottomRight) SpawnTileRotated(floorTileObj_C5, floorParent.transform, 180f, x, y);
                                    else if (!wallTopLeft && wallTopRight && !wallBottomLeft && wallBottomRight) SpawnTileRotated(floorTileObj_C5, floorParent.transform, 90f, x, y);
                                    else if (!wallTopLeft && wallTopRight && wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C9, floorParent.transform, x, y);
                                    else if (wallTopLeft && !wallTopRight && !wallBottomLeft && wallBottomRight) SpawnTileRotated(floorTileObj_C9, floorParent.transform, -90f, x, y);

                                    //3 sides
                                    else if (wallTopLeft && wallTopRight && wallBottomLeft && !wallBottomRight) SpawnTile(floorTileObj_C11, floorParent.transform, x, y);
                                    else if (wallTopLeft && !wallTopRight && wallBottomLeft && wallBottomRight) SpawnTileRotated(floorTileObj_C11, floorParent.transform, -90f, x, y);
                                    else if (!wallTopLeft && wallTopRight && wallBottomLeft && wallBottomRight) SpawnTileRotated(floorTileObj_C11, floorParent.transform, 180f, x, y);
                                    else if (wallTopLeft && wallTopRight && !wallBottomLeft && wallBottomRight) SpawnTileRotated(floorTileObj_C11, floorParent.transform, 90f, x, y);
                                    else if (wallTopLeft && wallTopRight && wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C31, floorParent.transform, x, y);
                                    else SpawnTile(floorTileObj_1, floorParent.transform, x, y);
                                }
                                else SpawnTile(floorTileObj_1, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }

                        //one side
                        else if (wallTop && !wallLeft && !wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners)
                                {
                                    if (wallBottomLeft && wallBottomRight) SpawnTile(floorTileObj_C17, floorParent.transform, x, y);
                                    else if (wallBottomLeft) SpawnTile(floorTileObj_C15, floorParent.transform, x, y);
                                    else if (wallBottomRight) SpawnTile(floorTileObj_C16, floorParent.transform, x, y);
                                    else SpawnTile(floorTileObj_2, floorParent.transform, x, y);
                                }
                                else SpawnTile(floorTileObj_2, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && wallLeft && !wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners)
                                {
                                    if (wallBottomRight && wallTopRight) SpawnTileRotated(floorTileObj_C17, floorParent.transform, -90f, x, y);
                                    else if (wallBottomRight) SpawnTileRotated(floorTileObj_C15, floorParent.transform, -90f, x, y);
                                    else if (wallTopRight) SpawnTileRotated(floorTileObj_C16, floorParent.transform, -90f, x, y);
                                    else SpawnTileRotated(floorTileObj_2, floorParent.transform, -90f, x, y);
                                }
                                else SpawnTileRotated(floorTileObj_2, floorParent.transform, -90f, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && !wallLeft && wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners)
                                {
                                    if (wallTopLeft && wallTopRight) SpawnTileRotated(floorTileObj_C17, floorParent.transform, 180f, x, y);
                                    else if (wallTopLeft) SpawnTileRotated(floorTileObj_C15, floorParent.transform, 180f, x, y);
                                    else if (wallTopRight) SpawnTileRotated(floorTileObj_C16, floorParent.transform, 180f, x, y);
                                    else SpawnTileRotated(floorTileObj_2, floorParent.transform, 180f, x, y);
                                }
                                else SpawnTileRotated(floorTileObj_2, floorParent.transform, 180f, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && !wallLeft && !wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners)
                                {
                                    if (wallTopLeft && wallBottomLeft) SpawnTileRotated(floorTileObj_C17, floorParent.transform, 90f, x, y);
                                    else if (wallTopLeft) SpawnTileRotated(floorTileObj_C15, floorParent.transform, 90f, x, y);
                                    else if (wallBottomLeft) SpawnTileRotated(floorTileObj_C16, floorParent.transform, 90f, x, y);
                                    else SpawnTileRotated(floorTileObj_2, floorParent.transform, 90f, x, y);
                                }
                                else SpawnTileRotated(floorTileObj_2, floorParent.transform, 90f, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }

                        //2 sides
                        else if (wallTop && wallLeft && !wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners && wallBottomRight) SpawnTile(floorTileObj_C27, floorParent.transform, x, y);
                                else SpawnTile(floorTileObj_6, floorParent.transform, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && wallLeft && wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners && wallTopRight) SpawnTileRotated(floorTileObj_C27, floorParent.transform, -90f, x, y);
                                else SpawnTileRotated(floorTileObj_6, floorParent.transform, -90f, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && !wallLeft && wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners && wallTopLeft) SpawnTileRotated(floorTileObj_C27, floorParent.transform, 180f, x, y);
                                else SpawnTileRotated(floorTileObj_6, floorParent.transform, 180f, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (wallTop && !wallLeft && !wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))
                            {
                                if (drawCorners && wallBottomLeft) SpawnTileRotated(floorTileObj_C27, floorParent.transform, 90f, x, y);
                                else SpawnTileRotated(floorTileObj_6, floorParent.transform, 90f, x, y);
                            }

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && wallLeft && !wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTile(floorTileObj_10, floorParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (wallTop && !wallLeft && wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTileRotated(floorTileObj_10, floorParent.transform, -90f, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }

                        //3 sides
                        else if (wallTop && wallLeft && !wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTile(floorTileObj_12, floorParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (wallTop && wallLeft && wallBottom && !wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTileRotated(floorTileObj_12, floorParent.transform, -90f, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (!wallTop && wallLeft && wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTileRotated(floorTileObj_12, floorParent.transform, 180f, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                        else if (wallTop && !wallLeft && wallBottom && wallRight)
                        {
                            if (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom)) SpawnTileRotated(floorTileObj_12, floorParent.transform, 90f, x, y);

                            if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                        }
                    }

                    else if (tiles[x, y] == tileType.wall)
                    {
                        bool floorTop = false;
                        bool floorLeft = false;
                        bool floorBottom = false;
                        bool floorRight = false;

                        bool floorTopLeft = false;
                        bool floorTopRight = false;
                        bool floorBottomLeft = false;
                        bool floorBottomRight = false;

                        if (y + 1 < levelSize.y) floorTop = tiles[x, y + 1] == tileType.floor;
                        if (x - 1 > 0) floorLeft = tiles[x - 1, y] == tileType.floor;
                        if (y - 1 > 0) floorBottom = tiles[x, y - 1] == tileType.floor;
                        if (x + 1 < levelSize.x) floorRight = tiles[x + 1, y] == tileType.floor;

                        if (x - 1 > 0 && y + 1 < levelSize.y) floorTopLeft = tiles[x - 1, y + 1] == tileType.floor;
                        if (x + 1 < levelSize.x && y + 1 < levelSize.y) floorTopRight = tiles[x + 1, y + 1] == tileType.floor;
                        if (x - 1 > 0 && y - 1 > 0) floorBottomLeft = tiles[x - 1, y - 1] == tileType.floor;
                        if (x + 1 < levelSize.x && y - 1 > 0) floorBottomRight = tiles[x + 1, y - 1] == tileType.floor;


                        //one side
                        if (floorTop && !floorLeft && !floorBottom && !floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C17, wallParent.transform, x, y);
                                else if (floorBottomLeft) SpawnTile(wallTileObj_C15, wallParent.transform, x, y);
                                else if (floorBottomRight) SpawnTile(wallTileObj_C16, wallParent.transform, x, y);
                                else SpawnTile(wallTileObj_2, wallParent.transform, x, y);
                            }
                            else SpawnTile(wallTileObj_2, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_2, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_2, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && floorLeft && !floorBottom && !floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorTopRight && floorBottomRight) SpawnTileRotated(wallTileObj_C17, wallParent.transform, -90f, x, y);
                                else if (floorBottomRight) SpawnTileRotated(wallTileObj_C15, wallParent.transform, -90f, x, y);
                                else if (floorTopRight) SpawnTileRotated(wallTileObj_C16, wallParent.transform, -90f, x, y);
                                else SpawnTileRotated(wallTileObj_2, wallParent.transform, -90f, x, y);
                            }
                            else SpawnTileRotated(wallTileObj_2, wallParent.transform, -90f, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_3, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_3, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && !floorLeft && floorBottom && !floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorTopRight && floorTopLeft) SpawnTileRotated(wallTileObj_C17, wallParent.transform, 180f, x, y);
                                else if (floorTopLeft) SpawnTileRotated(wallTileObj_C15, wallParent.transform, 180f, x, y);
                                else if (floorTopRight) SpawnTileRotated(wallTileObj_C16, wallParent.transform, 180f, x, y);
                                else SpawnTileRotated(wallTileObj_2, wallParent.transform, 180f, x, y);
                            }
                            else SpawnTileRotated(wallTileObj_2, wallParent.transform, 180f, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_4, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_4, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && !floorLeft && !floorBottom && floorRight)
                        {
                            if (drawCorners)
                            {
                                if (floorBottomLeft && floorTopLeft) SpawnTileRotated(wallTileObj_C17, wallParent.transform, 90f, x, y);
                                else if (floorTopLeft) SpawnTileRotated(wallTileObj_C15, wallParent.transform, 90f, x, y);
                                else if (floorBottomLeft) SpawnTileRotated(wallTileObj_C16, wallParent.transform, 90f, x, y);
                                else SpawnTileRotated(wallTileObj_2, wallParent.transform, 90f, x, y);
                            }
                            else SpawnTileRotated(wallTileObj_2, wallParent.transform, 90f, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_5, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_5, overlayParent.transform, x, y);
                        }

                        //2 sides
                        else if (floorTop && floorLeft && !floorBottom && !floorRight)
                        {
                            if (drawCorners && floorBottomRight) SpawnTile(wallTileObj_C27, wallParent.transform, x, y);
                            else SpawnTile(wallTileObj_6, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_6, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_6, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && floorLeft && floorBottom && !floorRight)
                        {
                            if (drawCorners && floorTopRight) SpawnTileRotated(wallTileObj_C27, wallParent.transform, -90f, x, y);
                            else SpawnTileRotated(wallTileObj_6, wallParent.transform, -90f, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_7, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_7, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && !floorLeft && floorBottom && floorRight)
                        {
                            if (drawCorners && floorTopLeft) SpawnTileRotated(wallTileObj_C27, wallParent.transform, 180f, x, y);
                            else SpawnTileRotated(wallTileObj_6, wallParent.transform, 180f, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_8, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_8, overlayParent.transform, x, y);
                        }
                        else if (floorTop && !floorLeft && !floorBottom && floorRight)
                        {
                            if (drawCorners && floorBottomLeft) SpawnTileRotated(wallTileObj_C27, wallParent.transform, 90f, x, y);
                            else SpawnTileRotated(wallTileObj_6, wallParent.transform, 90f, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_9, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_9, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && floorLeft && !floorBottom && floorRight)
                        {
                            SpawnTile(wallTileObj_10, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_10, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_10, overlayParent.transform, x, y);
                        }
                        else if (floorTop && !floorLeft && floorBottom && !floorRight)
                        {
                            SpawnTileRotated(wallTileObj_10, wallParent.transform, -90f, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_11, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_11, overlayParent.transform, x, y);
                        }

                        //3 sides
                        else if (floorTop && floorLeft && !floorBottom && floorRight)
                        {
                            SpawnTile(wallTileObj_12, wallParent.transform, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_12, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_12, overlayParent.transform, x, y);
                        }
                        else if (floorTop && floorLeft && floorBottom && !floorRight)
                        {
                            SpawnTileRotated(wallTileObj_12, wallParent.transform, -90f, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_13, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_13, overlayParent.transform, x, y);
                        }
                        else if (!floorTop && floorLeft && floorBottom && floorRight)
                        {
                            SpawnTileRotated(wallTileObj_12, wallParent.transform, 180f, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_14, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_14, overlayParent.transform, x, y);
                        }
                        else if (floorTop && !floorLeft && floorBottom && floorRight)
                        {
                            SpawnTileRotated(wallTileObj_12, wallParent.transform, 90f, x, y);

                            if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_15, overlayParent.transform, x, y);
                            else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_15, overlayParent.transform, x, y);
                        }

                        //corner walls
                        else if (spawnCornerWalls)
                        {
                            if (drawCorners)
                            {
                                //one side
                                if (floorTopLeft && !floorTopRight && !floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C1, wallParent.transform, x, y);
                                else if (!floorTopLeft && !floorTopRight && floorBottomLeft && !floorBottomRight) SpawnTileRotated(wallTileObj_C1, wallParent.transform, -90f, x, y);
                                else if (!floorTopLeft && !floorTopRight && !floorBottomLeft && floorBottomRight) SpawnTileRotated(wallTileObj_C1, wallParent.transform, 180f, x, y);
                                else if (!floorTopLeft && floorTopRight && !floorBottomLeft && !floorBottomRight) SpawnTileRotated(wallTileObj_C1, wallParent.transform, 90f, x, y);

                                //2 sides
                                else if (floorTopLeft && floorTopRight && !floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C5, wallParent.transform, x, y);
                                else if (floorTopLeft && !floorTopRight && floorBottomLeft && !floorBottomRight) SpawnTileRotated(wallTileObj_C5, wallParent.transform, -90f, x, y);
                                else if (!floorTopLeft && !floorTopRight && floorBottomLeft && floorBottomRight) SpawnTileRotated(wallTileObj_C5, wallParent.transform, 180f, x, y);
                                else if (!floorTopLeft && floorTopRight && !floorBottomLeft && floorBottomRight) SpawnTileRotated(wallTileObj_C5, wallParent.transform, 90f, x, y);
                                else if (!floorTopLeft && floorTopRight && floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C9, wallParent.transform, x, y);
                                else if (floorTopLeft && !floorTopRight && !floorBottomLeft && floorBottomRight) SpawnTileRotated(wallTileObj_C9, wallParent.transform, -90f, x, y);

                                //3 sides
                                else if (floorTopLeft && floorTopRight && floorBottomLeft && !floorBottomRight) SpawnTile(wallTileObj_C11, wallParent.transform, x, y);
                                else if (floorTopLeft && !floorTopRight && floorBottomLeft && floorBottomRight) SpawnTileRotated(wallTileObj_C11, wallParent.transform, -90f, x, y);
                                else if (!floorTopLeft && floorTopRight && floorBottomLeft && floorBottomRight) SpawnTileRotated(wallTileObj_C11, wallParent.transform, 180f, x, y);
                                else if (floorTopLeft && floorTopRight && !floorBottomLeft && floorBottomRight) SpawnTileRotated(wallTileObj_C11, wallParent.transform, 90f, x, y);
                                else if (floorTopLeft && floorTopRight && floorBottomLeft && floorBottomRight) SpawnTile(wallTileObj_C31, wallParent.transform, x, y);
                                else SpawnTile(wallTileObj_1, wallParent.transform, x, y);
                            }
                            else SpawnTile(wallTileObj_1, wallParent.transform, x, y);
                        }
                        else if (drawEmptyTiles) SpawnTile(emptyTileObj, emptyParent.transform, x, y);
                    }

                    else if (drawEmptyTiles) SpawnTile(emptyTileObj, emptyParent.transform, x, y);
                }
            }
        }
        
        private void SpawnTiles()
        {
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    if (tiles[x, y] == tileType.floor && (!deleteFloorBelowOverlay || (deleteFloorBelowOverlay && overlayTiles[x, y] != overlayType.floorPattern && overlayTiles[x, y] != overlayType.floorRandom))) SpawnTile(floorTileObj_1, floorParent.transform, x, y);
                    else if (tiles[x, y] == tileType.wall) SpawnTile(wallTileObj_1, wallParent.transform, x, y);
                    else if (drawEmptyTiles) SpawnTile(emptyTileObj, emptyParent.transform, x, y);

                    if (overlayTiles[x, y] == overlayType.floorPattern) SpawnTile(patternFloorTileObj, overlayParent.transform, x, y);
                    else if (overlayTiles[x, y] == overlayType.floorRandom) SpawnTile(randomFloorTileObj, overlayParent.transform, x, y);
                    else if (overlayTiles[x, y] == overlayType.wallPattern) SpawnTile(patternWallTileObj_1, overlayParent.transform, x, y);
                    else if (overlayTiles[x, y] == overlayType.wallRandom) SpawnTile(randomWallTileObj_1, overlayParent.transform, x, y);
                }
            }
        }


        private void GenerateFloorCollider()
        {
            BoxCollider levelFloorCollider = floorParent.AddComponent<BoxCollider>();

            levelFloorCollider.center = new Vector3((int)levelSize.x / 2 * tileSize, 0f, (int)levelSize.y / 2 * tileSize);
            levelFloorCollider.size = new Vector3(levelSize.x * tileSize, levelColliderHeight, levelSize.y * tileSize);
        }


        private void GenerateWallTileCollider()
        {
            wallParent.AddComponent<Rigidbody2D>();
            wallParent.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            wallParent.AddComponent<TilemapCollider2D>();
            wallParent.AddComponent<CompositeCollider2D>();
            wallParent.GetComponent<TilemapCollider2D>().usedByComposite = true;
        }


        private void GenerateWallCompositeCollider2D()
        {
            wallParent.AddComponent<Rigidbody2D>();
            wallParent.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            wallParent.AddComponent<CompositeCollider2D>();
        }


        private void RotateLevel()
        {
            if (levelRot == levelRotation.XY) this.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            else if (levelRot == levelRotation.ZY) this.transform.rotation = Quaternion.Euler(-90f, 0f, -90f);
            else this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        #endregion


        #region Utils

        private void SpawnTileRotated(GameObject _tileObj, Transform _parentTrm, float _rotation, int _posX, int _posY)
        {
            if(_tileObj != null)
            {
                GameObject instObj = GameObject.Instantiate(_tileObj, new Vector3(_posX * tileSize, 0, _posY * tileSize), _tileObj.transform.rotation * Quaternion.Euler(0f, _rotation, 0f));
                instObj.transform.parent = _parentTrm;
                instObj.transform.localPosition = new Vector3(instObj.transform.position.x, 0f, instObj.transform.position.z);
            }
        }
        
        private void SpawnTile(GameObject _tileObj, Transform _parentTrm, int _posX, int _posY)
        {
            if (_tileObj != null)
            {
                GameObject instObj = GameObject.Instantiate(_tileObj, new Vector3(_posX * tileSize, 0, _posY * tileSize), Quaternion.identity);
                instObj.transform.parent = _parentTrm;
                instObj.transform.localPosition = new Vector3(instObj.transform.position.x, 0f, instObj.transform.position.z);
            }
        }
        
        /// <summary>
        /// 레벨에서 특정 유형의 타일 수를 반환합니다.
        /// </summary>
        private int TileTypeNumber(tileType _tileType)
        {
            int count = 0;
            foreach (tileType tile in tiles)
            {
                if (tile == _tileType)
                    count++;
            }
            return count;
        }


        /// <summary>
        /// 'IsFloorTouchingWall' 함수는 주어진 위치의 타일이 '바닥' 타입이고 그 주변에 '벽' 타입의 타일이 없는지 확인합니다.
        /// 함수는 '바닥' 타일이 주변 8개의 타일(상하좌우 및 대각선) 중 어느 하나라도 '벽' 타일과 접하고 있지 않을 경우 true를 반환합니다.
        /// </summary>
        /// <param name="_posX">검사할 타일의 X 좌표입니다.</param>
        /// <param name="_posY">검사할 타일의 Y 좌표입니다.</param>
        /// <returns>주어진 위치의 타일이 '바닥' 타입이고 주변에 '벽' 타일이 없는 경우 true를 반환하고, 그렇지 않으면 false를 반환합니다.</returns>
        private bool IsFloorNotTouchingWall(int _posX, int _posY)
        {
            return (tiles[_posX, _posY] == tileType.floor &&
                    tiles[_posX + 1, _posY] != tileType.wall &&
                    tiles[_posX + 1, _posY + 1] != tileType.wall && 
                    tiles[_posX, _posY + 1] != tileType.wall &&
                    tiles[_posX - 1, _posY + 1] != tileType.wall &&
                    tiles[_posX - 1, _posY] != tileType.wall &&
                    tiles[_posX - 1, _posY - 1] != tileType.wall &&
                    tiles[_posX, _posY - 1] != tileType.wall &&
                    tiles[_posX + 1, _posY - 1] != tileType.wall);
        }

        #endregion


        #region Getters

        public tileType[,] GetTiles() { return tiles; }
        
        public overlayType[,] GetOverlayTiles() { return overlayTiles; }
        
        public Vector2Int GetLevelSize() { return levelSize; }
        
        public float GetTilesSize() { return tileSize; }
        
        public levelRotation GetLevelRotation() { return levelRot; }
        
        public genType GetGenerationType() { return generation; }

        #endregion
    }
}





#region Backup Code (향후 참조를 위해)

/*
 private void Setup()
 {
            // 레벨 경계에 벽을 생성하기 위해 주어진 레벨 크기에서 일정 부분을 뺀 값입니다.
            levelSizeCut = new Vector2Int(levelSize.x - 2, levelSize.y - 2);

            // 이는 최소한의 레벨 크기를 유지하기 위한 것입니다.
            // levelSizeCut.x나 levelSizeCut.y가 4보다 작으면 각각을 4로 설정하고 levelSize의 해당 값을 6으로 설정
            if (levelSizeCut.x < 4)
            {
                levelSizeCut.x = 4;
                levelSize.x = 6;
            }
            if (levelSizeCut.y < 4)
            {
                levelSizeCut.y = 4;
                levelSize.y = 6;
            }

            // 회전 확률 재계산
            // 그런 다음 합계가 100이 되도록 각 값을 비례적으로 조정합니다.
            float totalChances = pathMakerRotatesLeft + pathMakerRotatesRight + pathMakerRotatesBackwards; // 왼쪽, 오른쪽, 뒤로 회전할 총 확률을 계산합니다.
            pathMakerRotatesLeft = pathMakerRotatesLeft * 100f / totalChances; // 왼쪽으로 회전할 확률을 총 확률에 대한 비율로 재설정합니다.
            pathMakerRotatesRight = pathMakerRotatesRight * 100f / totalChances; // 오른쪽으로 회전할 확률을 총 확률에 대한 비율로 재설정합니다.
            pathMakerRotatesBackwards = pathMakerRotatesBackwards * 100f / totalChances; // 뒤로 회전할 확률을 총 확률에 대한 비율로 재설정합니다.
            

            // 2x2와 3x3 블록 확률 재계산
            float totalBlockChances = chunkChance2x2 + chunkChance3x3;
            chunkChance2x2 = chunkChance2x2 * 100 / totalBlockChances;
            chunkChance3x3 = chunkChance3x3 * 100 / totalBlockChances;

            // 회전 초기화
            this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            // 레벨 부모 생성
            if (generation == genType.generateObj)
                CreateObjParents();
            else
                CreateTilesParents();

            // 타일 인스턴스화
            tiles = new tileType[levelSize.x, levelSize.y];
            overlayTiles = new overlayType[levelSize.x, levelSize.y];

            // 모든 타일과 오버레이 타일을 비운 상태로 초기화
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    tiles[x, y] = tileType.empty;
                }
            }
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    overlayTiles[x, y] = overlayType.empty;
                }
            }

            // 첫 번째 pathMaker 생성
            pathMakers = new List<pathMaker>();
            pathMaker newGenerator = new pathMaker
            {
                direction = TurnPathMakers(Vector2.up),
                position = new Vector2(Mathf.RoundToInt(levelSizeCut.x / 2.0f), Mathf.RoundToInt(levelSizeCut.y / 2.0f))
            };
            pathMakers.Add(newGenerator);
}
*/

/*
private void CreateObjParents()
{
    // 바닥 오브젝트에 대한 부모 생성
    floorParent = new GameObject("floorParent")
    {
        transform =
        {
            parent = this.transform,
            localPosition = new Vector3(0f, floorOffset, 0f)
        }
    };

    // 벽 오브젝트에 대한 부모 생성
    wallParent = new GameObject("wallParent")
    {
        transform =
        {
            parent = this.transform,
            localPosition = new Vector3(0f, wallOffset, 0f)
        }
    };

    // 빈 공간에 대한 부모 생성
    if (drawEmptyTiles)
    {
        emptyParent = new GameObject("emptyParent")
        {
            transform =
            {
                parent = this.transform,
                localPosition = new Vector3(0f, emptyOffset, 0f)
            }
        };
    }

    // 오버레이 부모 생성
    if (drawFloorOverlayPatternTiles || drawFloorOverlayRandomTiles || drawWallOverlayPatternTiles || drawWallOverlayRandomTiles)
    {
        overlayParent = new GameObject("overlayParent")
        {
            transform =
            {
                parent = this.transform,
                localPosition = new Vector3(0f, overlayOffset, 0f)
            }
        };
    }
}
*/

/*private void CreateTilesParents()
        {
            gridParent = new GameObject("gridParent");  
            gridParent.AddComponent<Grid>();      
            gridParent.GetComponent<Grid>().cellSwizzle = GridLayout.CellSwizzle.XYZ;  
            gridParent.GetComponent<Grid>().cellSize = new Vector3(tileSize, tileSize, tileSize);  
            gridParent.transform.parent = this.transform;
            
            floorParent = new GameObject("floorParent");
            floorParent.AddComponent<Tilemap>();
            floorParent.AddComponent<TilemapRenderer>();
            floorParent.GetComponent<Tilemap>().orientation = Tilemap.Orientation.XY;
            floorParent.transform.parent = gridParent.transform;
            floorParent.transform.position = new Vector3(0f, 0f, floorOffset);

            wallParent = new GameObject("wallParent");
            wallParent.AddComponent<Tilemap>();
            wallParent.AddComponent<TilemapRenderer>();
            wallParent.GetComponent<Tilemap>().orientation = Tilemap.Orientation.XY;
            wallParent.transform.parent = gridParent.transform;
            wallParent.transform.position = new Vector3(0f, 0f, wallOffset);

            if (drawEmptyTiles)
            {
                emptyParent = new GameObject("emptyParent");
                emptyParent.AddComponent<Tilemap>();
                emptyParent.AddComponent<TilemapRenderer>();
                emptyParent.GetComponent<Tilemap>().orientation = Tilemap.Orientation.XY;
                emptyParent.transform.parent = gridParent.transform;
                emptyParent.transform.position = new Vector3(0f, 0f, emptyOffset);
            }

            if (drawFloorOverlayPatternTiles || drawFloorOverlayRandomTiles || drawWallOverlayPatternTiles || drawWallOverlayRandomTiles)
            {
                overlayParent = new GameObject("overlayParent");
                overlayParent.AddComponent<Tilemap>();
                overlayParent.AddComponent<TilemapRenderer>();
                overlayParent.GetComponent<Tilemap>().orientation = Tilemap.Orientation.XY;
                overlayParent.transform.parent = gridParent.transform;
                overlayParent.transform.position = new Vector3(0f, 0f, overlayOffset);
                overlayParent.GetComponent<TilemapRenderer>().sortingOrder = 1;
            }
        }*/

/*
private void IteratePathMakers()
{
    // 모든 경로 생성기에 대해 반복
    for (int i = 0; i < pathMakers.Count; i++)
    {
        // 파괴: 무작위로 경로 생성기를 파괴합니다. 
        // 하나 이상의 경로 생성기가 있을 때만 파괴하며, 그 확률은 pathMakerDestructionChance에 의해 결정됩니다.
        if (Random.Range(0, 100) < pathMakerDestructionChance && pathMakers.Count > 1)
        {
            pathMakers.RemoveAt(i);
            break;
        }

        // 회전: 무작위로 경로 생성기의 방향을 변경합니다.
        // 변경 확률은 pathMakerRotationChance에 의해 결정됩니다.
        if (Random.Range(0, 100) < pathMakerRotationChance)
        {
            pathMaker currentPathMaker = pathMakers[i];
            currentPathMaker.direction = TurnPathMakers(currentPathMaker.direction);
            pathMakers[i] = currentPathMaker;
        }

        // 생성: 무작위로 새로운 경로 생성기를 생성합니다. 
        // 생성 확률은 pathMakerSpawnChance에 의해 결정되며, 최대 경로 생성기 수 (pathMakerMaxDensity)를 초과하지 않습니다.
        if (Random.Range(0, 100) < pathMakerSpawnChance && pathMakers.Count < pathMakerMaxDensity)
        {
            pathMaker currentPathMaker = new pathMaker();
            currentPathMaker.direction = TurnPathMakers(pathMakers[i].direction);
            currentPathMaker.position = pathMakers[i].position;
            pathMakers.Add(currentPathMaker);
        }
    }
}
*/

/*
private void GenerateBlock()
{
    // 각 경로 제작자를 반복합니다.
    for (int i = 0; i < pathMakers.Count; i++)
    {
        // 패스 메이커의 위치에 블록이 생성되어야 하는지 확인합니다.
        if (Random.Range(0, 100) < chunkSpawnChance)
        {
            pathMaker currentPathMaker = pathMakers[i];

            // 생성할 블록 유형을 확인하십시오: 2x2 또는 3x3.
            if (Random.Range(0, 100) < chunkChance2x2)
            {
                // 경로 제작자의 위치에 2x2 블록을 생성합니다.
                tiles[(int)currentPathMaker.position.x + 1, (int)currentPathMaker.position.y] = tileType.floor;
                tiles[(int)currentPathMaker.position.x, (int)currentPathMaker.position.y + 1] = tileType.floor;
                tiles[(int)currentPathMaker.position.x + 1, (int)currentPathMaker.position.y + 1] = tileType.floor;
            }
            // 경로 제작자의 위치에 3x3 블록을 생성합니다.
            else  
            {
                tiles[(int)currentPathMaker.position.x + 1, (int)currentPathMaker.position.y] = tileType.floor;
                tiles[(int)currentPathMaker.position.x + 2, (int)currentPathMaker.position.y] = tileType.floor;
                tiles[(int)currentPathMaker.position.x, (int)currentPathMaker.position.y + 1] = tileType.floor;
                tiles[(int)currentPathMaker.position.x + 1, (int)currentPathMaker.position.y + 1] = tileType.floor;
                tiles[(int)currentPathMaker.position.x + 2, (int)currentPathMaker.position.y + 1] = tileType.floor;
                tiles[(int)currentPathMaker.position.x, (int)currentPathMaker.position.y + 2] = tileType.floor;
                tiles[(int)currentPathMaker.position.x + 1, (int)currentPathMaker.position.y + 2] = tileType.floor;
                tiles[(int)currentPathMaker.position.x + 2, (int)currentPathMaker.position.y + 2] = tileType.floor;
            }
        }
    }
}
*/

/*
private void GenerateWall()
{
    // 모든 타일을 순회합니다.
    for (int x = 0; x < levelSize.x - 1; x++)
    {
        for (int y = 0; y < levelSize.y - 1; y++)
        {
            // 현재 타일이 'floor' 타입이라면 주변 타일을 검사합니다.
            if (tiles[x, y] == tileType.floor)
            {
                // 주변 타일이 비어있다면, 그 위치에 'wall' 타입을 할당합니다.
                // 각각 오른쪽, 오른쪽 상단, 상단, 왼쪽 상단, 왼쪽, 왼쪽 하단, 하단, 오른쪽 하단에 대해 검사하고 할당합니다.
                if (tiles[x + 1, y] == tileType.empty)
                    tiles[x + 1, y] = tileType.wall;
                        
                if (tiles[x + 1, y + 1] == tileType.empty && spawnCornerWalls)
                    tiles[x + 1, y + 1] = tileType.wall;
                        
                if (tiles[x, y + 1] == tileType.empty)
                    tiles[x, y + 1] = tileType.wall;
                        
                if (tiles[x - 1, y + 1] == tileType.empty && spawnCornerWalls)
                    tiles[x - 1, y + 1] = tileType.wall;
                        
                if (tiles[x - 1, y] == tileType.empty) 
                    tiles[x - 1, y] = tileType.wall;
                        
                if (tiles[x - 1, y - 1] == tileType.empty && spawnCornerWalls) 
                    tiles[x - 1, y - 1] = tileType.wall;
                        
                if (tiles[x, y - 1] == tileType.empty) 
                    tiles[x, y - 1] = tileType.wall;
                        
                if (tiles[x + 1, y - 1] == tileType.empty && spawnCornerWalls)
                    tiles[x + 1, y - 1] = tileType.wall;
            }
        }
    }

    // 고립된 벽을 제거하는 메서드를 호출합니다.
    RemoveIsolatedWalls();
            
    // 설정에 따라 자연스럽지 않은 벽을 제거하는 메서드를 호출합니다.
    if (removeUnnaturalWalls)
        RemoveUnnaturalWalls();
}
*/

#endregion