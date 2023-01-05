using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    [SerializeField]
    private Sprite[] images;
    [SerializeField]
    private StageData StageData;
    [SerializeField]
    private float delayTime = 3.0f;
    private LayerMask tileLayer;
    private float rayDistance = 0.55f;
    private Vector2 moveDirection = Vector2.right;
    private Direction direction = Direction.Right;
    private Direction nextDirection = Direction.None;

    private Movement2D movement2D;
    private AroundWrap aroundWrap;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        movement2D = GetComponent<Movement2D>();
        aroundWrap = GetComponent<AroundWrap>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tileLayer = 1 << LayerMask.NameToLayer("Tile");

        //이동 방향을 임의로 설정
        SetMoveDirectionByRandom();
    }
    void Update()
    {
        //2. 이동방향에 광선 발사(장애물 검사)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, rayDistance, tileLayer);
        if(hit.transform == null)
        {
            //MoveTo() 함수에 이동방향을 매개변수로 전달해 이동
            movement2D.MoveTo(moveDirection);
            //화면 밖으로 나가게 되면 반대편에서 등작
            aroundWrap.UpdateAroundWrap();
        }
        else
        {
            SetMoveDirectionByRandom();
        }
    }

    private void SetMoveDirection(Direction direction)
    {
        //이동 방향 설정
        this.direction = direction;
        //Vector3 타입의 이동 방향 값 설정
        moveDirection = Vector3FromEnum(this.direction);
        //이동 방향에 맞춰 이미지 변경
        spriteRenderer.sprite = images[(int)this.direction];

        //모든 코루틴 중지
        StopAllCoroutines();
        //일정 시간동안 같은 방향으로 이동할 경우 방향을 바꿈
        StartCoroutine("SetMoveDirectionByTime");
    }

    private void SetMoveDirectionByRandom()
    {
        //이동 방향을 임의로 설정
        direction = (Direction)Random.Range(0, (int)Direction.Count);
        SetMoveDirection(direction);
    }

    private IEnumerator SetMoveDirectionByTime()
    {
        yield return new WaitForSeconds(delayTime);
        //현재 이동 방향이 Right or Left이면 direction % 2는 0으로 설정
        //다음 이동 방향(nextDirection)은 Up(1) or Down(3)으로 설정

        //현재 이동 방향이 Up or Down이면 Direction % 2는 1로
        //다음 이동 방향(nextDirection)은 Right(0) or Left(2)로 설정
        int dir = Random.Range(0, 2);
        nextDirection = (Direction)(dir * 2 + 1 - (int)direction % 2);
        //해당 방향으로 이동이 가능한지 체크한 후 실제 방향을 변경하는 코루틴 함수
        StartCoroutine("CheckBlockedNextMoveDirection");
    }

    private IEnumerator CheckBlockedNextMoveDirection()
    {
        while (true)
        {
            Vector3[] directions = new Vector3[3];
            bool[] isPossibleMoves = new bool[3];

            directions[0] = Vector3FromEnum(nextDirection);
            //nextDirection 이동 방향이 오른쪽 또는 왼쪽일 때,
            if(directions[0].x != 0)
            {
                directions[1] = directions[0] + new Vector3(0, 0.65f, 0);
                directions[2] = directions[0] + new Vector3(0, -0.65f, 0);
            }
            //nextDirection 이동방향이 위 또는 아래일 때
            else if (directions[0].y != 0)
            {
                directions[1] = directions[0] + new Vector3(0.65f, 0, 0);
                directions[2] = directions[0] + new Vector3(-0.65f, 0, 0);
            }
            //nextDirection 이동 방향으로 이동이 가능한지 판별하기 위해 3개의 광선 발사
            int possibleCount = 0;
            for(int i = 0; i < 3; ++i)
            {
                if (i == 0)
                {
                    isPossibleMoves[i] = Physics2D.Raycast(transform.position, directions[i], 0.5f, tileLayer);
                    Debug.DrawLine(transform.position, transform.position + directions[i] * 0.5f, Color.yellow);
                }
                else
                {
                    isPossibleMoves[i] = Physics2D.Raycast(transform.position, directions[i], 0.7f, tileLayer);
                    Debug.DrawLine(transform.position, transform.position + directions[i] * 0.7f, Color.yellow);
                }
                if(isPossibleMoves[i] == false)
                {
                    possibleCount++;
                }
            }

            //3개의 광선에 부딪히는 오브젝트가 없을 때(이동 방향에 장애물이 없다는 뜻)
            if(possibleCount == 3)
            {
                //외곽으로 나갔을 때, 이동하면 안되기 때문에 스테이지 범위 내에 있는지 검사
                if(transform.position.x > StageData.LimitMin.x && transform.position.x < StageData.LimitMax.x && transform.position.y > StageData.LimitMin.y && transform.position.y < StageData.LimitMax.y)
                {
                    //이동 방향을 nextDirection으로 변경
                    SetMoveDirection(nextDirection);
                    //nextDirection은 None으로 설정
                    nextDirection = Direction.None;
                    //코루틴 중지
                    break;
                }
            }
            yield return null; 
        }
    }

    private Vector3 Vector3FromEnum(Direction state)
    {
        Vector3 direction = Vector3.zero;

        switch (state)
        {
            case Direction.Up:
                direction = Vector3.up;
                break;
            case Direction.Left:
                direction = Vector3.left;
                break;
            case Direction.Right:
                direction = Vector3.right;
                break;
            case Direction.Down:
                direction = Vector3.down;
                break;
        }
        return direction;
    }
}
