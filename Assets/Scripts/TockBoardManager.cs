using System.Collections.Generic;
using UnityEngine;

public class TockBoardManager : MonoBehaviour
{
    [Range(10, 30)]
    public int m_PlayerToPlayerDist;
    [Range(2, 10)]
    public int m_PlayerCount;
    [Range(1, 6)]
    public int m_BallCount;

    [System.NonSerialized]
    public int m_BoardSize;

    // 0 is the start for team 1 balls and 70 is the bases entry (2 before because board is circular), 18 for team 2 balls and 16 for bases entry, etc.
    // Value represents which team the ball belongs to, 0 is team 1, 1 is team 2 etc. -1 is no ball on this cell
    [System.NonSerialized]
    public int[] m_Board;
    // 0 to 3 is team 1, 4 to 7 is team 2, etc. (4 bases per team)
    // Value represents if there's a ball or not
    [System.NonSerialized]
    public bool[] m_Bases;
    // Same as before for indexes (4 balls per team)
    // Value indicates current position, 0 to 71 being on board and 72 and 73 to 76 being start and bases respectivly
    [System.NonSerialized]
    public int[] m_PlayerBalls;

    // Value represents which team's turn it is, 0 is team 1, 1 is team 2 etc.
    [System.NonSerialized]
    public int m_CurrentPlayer;
    // For each of the current player's balls, -1 if move isn't valid and the new position if it is
    [System.NonSerialized]
    public int[] m_ValidMoves;

    private void Start()
    {
        m_BoardSize = m_PlayerToPlayerDist * m_PlayerCount;

        resetGame();
    }

    public void playCurrentComputerTurn()
    {
        int diceRoll = rollDice();

        print(diceRoll);

        findValidMoves(diceRoll);

        chooseComputerMove();

        if (checkVictoryConditions())
        {
            resetGame();
        }
        else
        {
            m_CurrentPlayer = (m_CurrentPlayer + 1) % m_PlayerCount;
        }
    }

    private int rollDice()
    {
        return Random.Range(1, 7);
    }

    private void findValidMoves(int diceRoll)
    {
        for (int i = 0; i < m_ValidMoves.Length; i++)
        {
            int ballIndex = m_CurrentPlayer * m_BallCount + i;
            int currentPos = m_PlayerBalls[ballIndex];
            bool validFound = false;

            if (currentPos == m_BoardSize && (diceRoll == 1 || diceRoll == 6)
                && m_Board[0 + (m_PlayerToPlayerDist * m_CurrentPlayer)] != m_CurrentPlayer)
            {
                m_ValidMoves[i] = 0 + (m_PlayerToPlayerDist * m_CurrentPlayer);
                validFound = true;
            }
            else
            {
                int basesCheck = 0;
                int basePos = 0;

                if (((currentPos + 2) % m_BoardSize) % m_PlayerToPlayerDist == 0 &&
                    ((currentPos + 2) % m_BoardSize) / m_PlayerToPlayerDist == m_CurrentPlayer)
                {
                    basesCheck = diceRoll;
                }
                else if (currentPos < m_BoardSize)
                {
                    for (int j = 1; j <= diceRoll; j++)
                    {
                        int newPos = (currentPos + j) % m_BoardSize;
                        int cellTeam = m_Board[newPos];

                        if (cellTeam == m_CurrentPlayer)
                        {
                            break;
                        }
                        else if (cellTeam == newPos / m_PlayerToPlayerDist && newPos % m_PlayerToPlayerDist == 0)
                        {
                            break;
                        }
                        else if (((newPos + 2) % m_BoardSize) % m_PlayerToPlayerDist == 0 &&
                            ((newPos + 2) % m_BoardSize) / m_PlayerToPlayerDist == m_CurrentPlayer && j < diceRoll)
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
                else if (currentPos > m_BoardSize)
                {
                    basesCheck = diceRoll;
                    basePos = currentPos - m_BoardSize;
                }

                for (int j = 0; j < basesCheck; j++)
                {
                    int newPos = basePos + j;
                    if (newPos >= m_BallCount) { break; }
                    if (m_Bases[m_CurrentPlayer * m_PlayerCount + newPos]) { break; }
                    if (j == basesCheck - 1)
                    {
                        m_ValidMoves[i] = m_BoardSize + 1 + newPos;
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

            if (move < m_BoardSize && m_Board[move] >= 0 && pickImportance < 4)
            {
                currentPick = i;
                pickImportance = 4;
            }
            else if (m_PlayerBalls[m_CurrentPlayer * m_BallCount + i] == m_BoardSize && pickImportance < 3)
            {
                currentPick = i;
                pickImportance = 3;
            }
            else if (move > m_BoardSize && pickImportance < 2)
            {
                currentPick = i;
                pickImportance = 2;
            }
            else if (m_PlayerBalls[m_CurrentPlayer * m_BallCount + i] != m_CurrentPlayer * m_PlayerToPlayerDist && pickImportance < 1)
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
            int oldPos = m_PlayerBalls[m_CurrentPlayer * m_BallCount + currentPick];
            if (oldPos < m_BoardSize)
            {
                m_Board[oldPos] = -1;
            }
            else if (oldPos > m_BoardSize)
            {
                m_Bases[m_CurrentPlayer * m_BallCount + oldPos - (m_BoardSize + 1)] = false;
            }

            if (pickedMove < m_BoardSize)
            {
                if (m_Board[pickedMove] >= 0)
                {
                    for (int j = m_Board[pickedMove] * m_BallCount; j < (m_Board[pickedMove] + 1) * m_BallCount; j++)
                    {
                        if (m_PlayerBalls[j] == pickedMove)
                        {
                            m_PlayerBalls[j] = m_BoardSize;
                            break;
                        }
                    }
                }

                m_Board[pickedMove] = m_CurrentPlayer;
            }
            else
            {
                m_Bases[m_CurrentPlayer * m_BallCount + pickedMove - (m_BoardSize + 1)] = true;
            }

            m_PlayerBalls[m_CurrentPlayer * m_BallCount + currentPick] = pickedMove;
        }
    }

    private void resetGame()
    {
        m_Board = new int[m_BoardSize];
        for (int i = 0; i < m_Board.Length; i++)
        {
            m_Board[i] = -1;
        }

        m_Bases = new bool[m_BallCount * m_PlayerCount];
        for (int i = 0; i < m_Bases.Length; i++)
        {
            m_Bases[i] = false;
        }

        m_PlayerBalls = new int[m_BallCount * m_PlayerCount];
        for (int i = 0; i < m_PlayerBalls.Length; i++)
        {
            m_PlayerBalls[i] = m_BoardSize;
        }

        m_CurrentPlayer = 0;

        m_ValidMoves = new int[m_BallCount];
        for (int i = 0; i < m_ValidMoves.Length; i++)
        {
            m_ValidMoves[i] = -1;
        }
    }

    private bool checkVictoryConditions()
    {
        for (int i = m_CurrentPlayer * m_BallCount; i < (m_CurrentPlayer + 1) * m_BallCount; i++)
        {
            if(!m_Bases[i]) { return false; }
        }

        return true;
    }
}
