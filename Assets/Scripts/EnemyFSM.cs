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

        //�̵� ������ ���Ƿ� ����
        SetMoveDirectionByRandom();
    }
    void Update()
    {
        //2. �̵����⿡ ���� �߻�(��ֹ� �˻�)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, rayDistance, tileLayer);
        if(hit.transform == null)
        {
            //MoveTo() �Լ��� �̵������� �Ű������� ������ �̵�
            movement2D.MoveTo(moveDirection);
            //ȭ�� ������ ������ �Ǹ� �ݴ����� ����
            aroundWrap.UpdateAroundWrap();
        }
        else
        {
            SetMoveDirectionByRandom();
        }
    }

    private void SetMoveDirection(Direction direction)
    {
        //�̵� ���� ����
        this.direction = direction;
        //Vector3 Ÿ���� �̵� ���� �� ����
        moveDirection = Vector3FromEnum(this.direction);
        //�̵� ���⿡ ���� �̹��� ����
        spriteRenderer.sprite = images[(int)this.direction];

        //��� �ڷ�ƾ ����
        StopAllCoroutines();
        //���� �ð����� ���� �������� �̵��� ��� ������ �ٲ�
        StartCoroutine("SetMoveDirectionByTime");
    }

    private void SetMoveDirectionByRandom()
    {
        //�̵� ������ ���Ƿ� ����
        direction = (Direction)Random.Range(0, (int)Direction.Count);
        SetMoveDirection(direction);
    }

    private IEnumerator SetMoveDirectionByTime()
    {
        yield return new WaitForSeconds(delayTime);
        //���� �̵� ������ Right or Left�̸� direction % 2�� 0���� ����
        //���� �̵� ����(nextDirection)�� Up(1) or Down(3)���� ����

        //���� �̵� ������ Up or Down�̸� Direction % 2�� 1��
        //���� �̵� ����(nextDirection)�� Right(0) or Left(2)�� ����
        int dir = Random.Range(0, 2);
        nextDirection = (Direction)(dir * 2 + 1 - (int)direction % 2);
        //�ش� �������� �̵��� �������� üũ�� �� ���� ������ �����ϴ� �ڷ�ƾ �Լ�
        StartCoroutine("CheckBlockedNextMoveDirection");
    }

    private IEnumerator CheckBlockedNextMoveDirection()
    {
        while (true)
        {
            Vector3[] directions = new Vector3[3];
            bool[] isPossibleMoves = new bool[3];

            directions[0] = Vector3FromEnum(nextDirection);
            //nextDirection �̵� ������ ������ �Ǵ� ������ ��,
            if(directions[0].x != 0)
            {
                directions[1] = directions[0] + new Vector3(0, 0.65f, 0);
                directions[2] = directions[0] + new Vector3(0, -0.65f, 0);
            }
            //nextDirection �̵������� �� �Ǵ� �Ʒ��� ��
            else if (directions[0].y != 0)
            {
                directions[1] = directions[0] + new Vector3(0.65f, 0, 0);
                directions[2] = directions[0] + new Vector3(-0.65f, 0, 0);
            }
            //nextDirection �̵� �������� �̵��� �������� �Ǻ��ϱ� ���� 3���� ���� �߻�
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

            //3���� ������ �ε����� ������Ʈ�� ���� ��(�̵� ���⿡ ��ֹ��� ���ٴ� ��)
            if(possibleCount == 3)
            {
                //�ܰ����� ������ ��, �̵��ϸ� �ȵǱ� ������ �������� ���� ���� �ִ��� �˻�
                if(transform.position.x > StageData.LimitMin.x && transform.position.x < StageData.LimitMax.x && transform.position.y > StageData.LimitMin.y && transform.position.y < StageData.LimitMax.y)
                {
                    //�̵� ������ nextDirection���� ����
                    SetMoveDirection(nextDirection);
                    //nextDirection�� None���� ����
                    nextDirection = Direction.None;
                    //�ڷ�ƾ ����
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
