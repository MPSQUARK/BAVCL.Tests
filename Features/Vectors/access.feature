Feature: Accessing values from a vector

    Background: Setup
        Given I have a gpu
        And I create the following vector
            | Alias              | IsCached | Columns | Values                              |
            | Access Test Vector | true     | 5       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |

    Scenario: I access a specific value from a vector
        When I access index 5 from the vector with alias Access Test Vector
        Then I should get the value 5

    Scenario: I access a column of values from a vector
        When I access column 2 from the vector with alias Access Test Vector
        Then I should get the following values
            | Value |
            | 2     |
            | 7     |
            | 12    |

    Scenario: I access a row of values from a vector
        When I access row 2 from the vector with alias Access Test Vector
        Then I should get the following values
            | Value |
            | 6     |
            | 7     |
            | 8     |
            | 9     |
            | 10    |


