using System.Collections.Generic;
using UnityEngine;

public class TockBoardManager : MonoBehaviour
{
    public const int PLAYER_TO_PLAYER_DIST = 18;
    public const int PLAYER_COUNT = 4;
    public const int BOARD_SIZE = PLAYER_TO_PLAYER_DIST * PLAYER_COUNT;
    public const int BALL_COUNT = 4;

    // 0 is the start for team 1 balls and 70 is the bases entry (2 before because board is circular), 18 for team 2 balls and 16 for bases entry, etc.
    // Value represents which team the ball belongs to, 0 is team 1, 1 is team 2 etc. -1 is no ball on this cell
    public int[] m_Board;
    // 0 to 3 is team 1, 4 to 7 is team 2, etc. (4 bases per team)
    // Value represents if there's a ball or not
    public bool[] m_Bases;
    // Same as before for indexes (4 balls per team)
    // Value indicates current position, 0 to 71 being on board and 72 and 73 to 76 being start and bases respectivly
    public int[] m_PlayerBalls;

    // Value represents which team's turn it is, 0 is team 1, 1 is team 2 etc.
    public int m_CurrentPlayer;
    // For each of the current player's balls, -1 if move isn't valid and the new position if it is
    public int[] m_ValidMoves;

    private void Start()
    {
        resetGame();
    }

    public void playCurrentComputerTurn()
    {
        int diceRoll = rollDice();

        print(diceRoll);

        findValidMoves(diceRoll);

        chooseComputerMove();

        m_CurrentPlayer = (m_CurrentPlayer + 1) % PLAYER_COUNT;
    }

    private int rollDice()
    {
        return Random.Range(1, 7);
    }

    private void findValidMoves(int diceRoll)
    {
        for (int i = 0; i < m_ValidMoves.Length; i++)
        {
            int ballIndex = m_CurrentPlayer * BALL_COUNT + i;
            int currentPos = m_PlayerBalls[ballIndex];
            bool validFound = false;

            if (currentPos == BOARD_SIZE && (diceRoll == 1 || diceRoll == 6)
                && m_Board[0 + (PLAYER_TO_PLAYER_DIST * m_CurrentPlayer)] != m_CurrentPlayer)
            {
                m_ValidMoves[i] = 0 + (PLAYER_TO_PLAYER_DIST * m_CurrentPlayer);
                validFound = true;
            }
            else
            {
                int basesCheck = 0;
                int basePos = 0;

                if (currentPos < BOARD_SIZE)
                {
                    for (int j = 1; j <= diceRoll; j++)
                    {
                        int newPos = (currentPos + j) % (BOARD_SIZE);
                        int cellTeam = m_Board[newPos];

                        if (cellTeam == m_CurrentPlayer)
                        {
                            break;
                        }
                        else if (cellTeam == newPos / PLAYER_TO_PLAYER_DIST && newPos % PLAYER_TO_PLAYER_DIST == 0)
                        {
                            break;
                        }
                        else if (((newPos + 2) % (BOARD_SIZE)) % PLAYER_TO_PLAYER_DIST == 0 &&
                            ((newPos + 2) % (BOARD_SIZE)) / PLAYER_TO_PLAYER_DIST == m_CurrentPlayer && j < diceRoll)
                        {
                            basesCheck = diceRoll - j;
                            break;
                        }

                        if (j == diceRoll)
                        {
                            m_ValidMoves[i] = newPos;
                            validFound = true;
                        }
                    }
                }
                else if (currentPos > BOARD_SIZE)
                {
                    basesCheck = diceRoll;
                    basePos = currentPos - BOARD_SIZE;
                }

                for (int j = 0; j < basesCheck; j++)
                {
                    int newPos = basePos + j;
                    if (newPos >= BALL_COUNT) { break; }
                    if (m_Bases[m_CurrentPlayer * PLAYER_COUNT + newPos]) { break; }
                    if (j == basesCheck - 1)
                    {
                        m_ValidMoves[i] = BOARD_SIZE + 1 + newPos;
                        validFound = true;
                    }
                }
            }

            if (!validFound)
            {
                m_ValidMoves[i] = -1;
            }
        }
    }

    private void chooseComputerMove()
    {
        int currentPick = -1;
        int pickImportance = -1;

        for (int i = 0; i < m_ValidMoves.Length; i++)
        {
            int move = m_ValidMoves[i];
            
            if(move < 0) { continue; }

            if (move < BOARD_SIZE && m_Board[move] >= 0 && pickImportance < 4)
            {
                currentPick = i;
                pickImportance = 4;
            }
            else if (m_PlayerBalls[m_CurrentPlayer * BALL_COUNT + i] == BOARD_SIZE && pickImportance < 3)
            {
                currentPick = i;
                pickImportance = 3;
            }
            else if (move > BOARD_SIZE && pickImportance < 2)
            {
                currentPick = i;
                pickImportance = 2;
            }
            else if (m_PlayerBalls[m_CurrentPlayer * BALL_COUNT + i] != m_CurrentPlayer * PLAYER_TO_PLAYER_DIST && pickImportance < 1)
            {
                currentPick = i;
                pickImportance = 1;
            }
            else if (pickImportance < 0)
            {
                currentPick = i;
                pickImportance = 0;
            }
        }

        if (currentPick >= 0) {
            int pickedMove = m_ValidMoves[currentPick];
            int oldPos = m_PlayerBalls[m_CurrentPlayer * BALL_COUNT + currentPick];
            if (oldPos < BOARD_SIZE)
            {
                m_Board[oldPos] = -1;
            }
            else if (oldPos > BOARD_SIZE)
            {
                m_Bases[m_CurrentPlayer * BALL_COUNT + oldPos - (BOARD_SIZE + 1)] = false;
            }

            if (pickedMove < BOARD_SIZE)
            {
                if (m_Board[pickedMove] >= 0)
                {
                    for (int j = m_Board[pickedMove] * BALL_COUNT; j < (m_Board[pickedMove] + 1) * BALL_COUNT; j++)
                    {
                        if (m_PlayerBalls[j] == pickedMove)
                        {
                            m_PlayerBalls[j] = BOARD_SIZE;
                            break;
                        }
                    }
                }

                m_Board[pickedMove] = m_CurrentPlayer;
            }
            else
            {
                m_Bases[m_CurrentPlayer * BALL_COUNT + pickedMove - (BOARD_SIZE + 1)] = true;
            }

            m_PlayerBalls[m_CurrentPlayer * BALL_COUNT + currentPick] = pickedMove;
        }
    }

    private void resetGame()
    {
        m_Board = new int[BOARD_SIZE];
        for (int i = 0; i < m_Board.Length; i++)
        {
            m_Board[i] = -1;
        }

        m_Bases = new bool[BALL_COUNT * PLAYER_COUNT];
        for (int i = 0; i < m_Bases.Length; i++)
        {
            m_Bases[i] = false;
        }

        m_PlayerBalls = new int[BALL_COUNT * PLAYER_COUNT];
        for (int i = 0; i < m_PlayerBalls.Length; i++)
        {
            m_PlayerBalls[i] = BOARD_SIZE;
        }

        m_CurrentPlayer = 0;

        m_ValidMoves = new int[BALL_COUNT];
        for (int i = 0; i < m_ValidMoves.Length; i++)
        {
            m_ValidMoves[i] = -1;
        }
    }
}
