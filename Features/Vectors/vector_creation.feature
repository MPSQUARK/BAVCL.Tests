Feature: Vector Initialisation

    Vector creation.
    Can create cached and non-cached vectors. Values saved to both RAM & GRAM
    Can create empty vector of given length that is always cached. Values only saved to GRAM.

    Background: Setup
        Given I have a gpu

    Scenario: Cached Vector Creation
        When I create the following cached vector
            | Value   |
            | NaN     |
            | Inf     |
            | -Inf    |
            | 5.0     |
            | 0.1234  |
            | -0.2434 |
        Then the vector should have the following properties
            | Columns    | 0  |
            | Rows       | 1  |
            | Length     | 6  |
            | LiveCount  | 0  |
            | MemorySize | 24 |
        And it should be cached on the GPU
        And there should be 1 item stored on the GPU
        And the vector should have the following values on the CPU and GPU
            | Value   |
            | NaN     |
            | Inf     |
            | -Inf    |
            | 5.0     |
            | 0.1234  |
            | -0.2434 |

    Scenario: Non-cached Vector Creation
        When I create the following non-cached vector:
            | Value   |
            | NaN     |
            | Inf     |
            | -Inf    |
            | 5.0     |
            | 0.1234  |
            | -0.2434 |
        Then the vector should have the following properties
            | Columns    | 0  |
            | Rows       | 1  |
            | Length     | 6  |
            | LiveCount  | 0  |
            | MemorySize | 24 |
        And the vector should have the following values on the CPU:
            | Value   |
            | NaN     |
            | Inf     |
            | -Inf    |
            | 5.0     |
            | 0.1234  |
            | -0.2434 |
        And it should not be cached on the GPU

    Scenario: Create An Empty Vector Of Given Length
        When I create a zeros vector of length 5
        Then the vector should have the following properties:
            | Columns    | 0  |
            | Rows       | 1  |
            | Length     | 5  |
            | LiveCount  | 0  |
            | MemorySize | 24 |
        And the vector should have no values on the CPU
        And it should be cached on the GPU