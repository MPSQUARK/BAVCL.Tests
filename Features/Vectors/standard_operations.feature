Feature: Standard operations performed with vectors

    Background: Setup GPU
        Given I have a gpu

    Scenario: Vector addition
        Given I create the following vectors
            | Alias             | IsCached | Columns | Values                              |
            | Addition Vector A | true     | 0       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |
            | Addition Vector B | true     | 0       | 1,2,3,4,5,6,7,8,9,10,-1,-2,-2,-2,-2 |
        When I add the vectors "Addition Vector A" and "Addition Vector B" and store the result in "Addition Result Vector C"
        Then the vector "Addition Result Vector C" should have the following values
            | 2 | 4 | 6 | 8 | 10 | 12 | 14 | 16 | 18 | 20 | 10 | 10 | 11 | 12 | 13 |

    Scenario: Vector subtraction
        Given I create the following vectors
            | Alias                | IsCached | Columns | Values                               |
            | Subtraction Vector A | true     | 0       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15  |
            | Subtraction Vector B | true     | 0       | 2,2,2,2,2,2,2,2,-2,-2,-2,-2,-2,-2,-2 |
        When I subtract the vectors "Subtraction Vector A" and "Subtraction Vector B" and store the result in "Subtraction Result Vector C"
        Then the vector "Subtraction Result Vector C" should have the following values
            | -1 | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 11 | 12 | 13 | 14 | 15 | 16 | 17 |

    Scenario: Vector multiplication (element-wise) 1D vs 1D
        Given I create the following vectors
            | Alias                   | IsCached | Columns | Values                               |
            | Multiplication Vector A | true     | 0       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15  |
            | Multiplication Vector B | true     | 0       | 2,2,2,2,2,2,2,2,-2,-2,-2,-2,-2,-2,-2 |
        When I multiply the vectors "Multiplication Vector A" and "Multiplication Vector B" and store the result in "Multiplication Result Vector C"
        Then the vector "Multiplication Result Vector C" should have the following values
            | 2 | 4 | 6 | 8 | 10 | 12 | 14 | 16 | -18 | -20 | -22 | -24 | -26 | -28 | -30 |

    Scenario: Vector multiplication (wrapped-elementwise) 1D vs 1D
        Given I create the following vectors
            | Alias                   | IsCached | Columns | Values                              |
            | Multiplication Vector A | true     | 0       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |
            | Multiplication Vector B | true     | 0       | 2,2,2-2,-2                          |
        When I multiply the vectors "Multiplication Vector A" and "Multiplication Vector B" and store the result in "Multiplication Result Vector C"
        Then the vector "Multiplication Result Vector C" should have the following values
            | 2 | 4 | 6 | -8 | -10 | 12 | 14 | 16 | -18 | -20 | 22 | 24 | 26 | -28 | -30 |

    Scenario: Vector multiplication (wrapped-elementwise) 1D vs 2D
        Given I create the following vectors
            | Alias                   | IsCached | Columns | Values                              |
            | Multiplication Vector A | true     | 0       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |
            | Multiplication Vector B | true     | 5       | 2,2,2-2,-2                          |
        When I multiply the vectors "Multiplication Vector A" and "Multiplication Vector B" and store the result in "Multiplication Result Vector C"
        Then the vector "Multiplication Result Vector C" should have the following values
            | 2 | 4 | 6 | -8 | -10 | 12 | 14 | 16 | -18 | -20 | 22 | 24 | 26 | -28 | -30 |

    Scenario: Vector multiplication (mapwise) 1D vs 2D
        Given I create the following vectors
            | Alias                   | IsCached | Columns | Values                              |
            | Multiplication Vector A | true     | 0       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |
            | Multiplication Vector B | true     | 1       | 2,2,2-2,-2                          |
        When I multiply the vectors "Multiplication Vector A" and "Multiplication Vector B" and store the result in "Multiplication Result Vector C"
        Then the vector "Multiplication Result Vector C" should have the following values
            | col1 | col2 | col3 | col4 | col5 | col6 | col7 | col8 | col9 | col10 | col11 | col12 | col13 | col14 | col15 |
            | 2    | 4    | 6    | 8    | 10   | 12   | 14   | 16   | 18   | 20    | 22    | 24    | 26    | 28    | 30    |
            | 2    | 4    | 6    | 8    | 10   | 12   | 14   | 16   | 18   | 20    | 22    | 24    | 26    | 28    | 30    |
            | 2    | 4    | 6    | 8    | 10   | 12   | 14   | 16   | 18   | 20    | 22    | 24    | 26    | 28    | 30    |
            | -2   | -4   | -6   | -8   | -10  | -12  | -14  | -16  | -18  | -20   | -22   | -24   | -26   | -28   | -30   |
            | -2   | -4   | -6   | -8   | -10  | -12  | -14  | -16  | -18  | -20   | -22   | -24   | -26   | -28   | -30   |

    Scenario: Vector multiplication (grouped-elementwise) 2D vs 2D
        Given I create the following vectors
            | Alias                   | IsCached | Columns | Values                              |
            | Multiplication Vector A | true     | 5       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |
            | Multiplication Vector B | true     | 5       | 2,2,2-2,-2                          |
        When I multiply the vectors "Multiplication Vector A" and "Multiplication Vector B" and store the result in "Multiplication Result Vector C"
        Then the vector "Multiplication Result Vector C" should have the following values
            | 2 | 4 | 6 | -8 | -10 | 12 | 14 | 16 | -18 | -20 | 22 | 24 | 26 | -28 | -30 |

    Scenario: Vector multiplication (elementwise) 2D vs 2D
        Given I create the following vectors
            | Alias                   | IsCached | Columns | Values                              |
            | Multiplication Vector A | true     | 5       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |
            | Multiplication Vector B | true     | 5       | 1,2,3,4,5,6,7,8,9,10,-2,-3,-4,-5,-6 |
        When I multiply the vectors "Multiplication Vector A" and "Multiplication Vector B" and store the result in "Multiplication Result Vector C"
        Then the vector "Multiplication Result Vector C" should have the following values
            | 1 | 4 | 9 | 16 | 25 | 36 | 49 | 64 | 81 | 100 | -22 | -36 | -52 | -70 | -90 |

    Scenario: Vector multiplication (dot product) 1D vs 2D
        Given I create the following vectors
            | Alias                   | IsCached | Columns | Values                              |
            | Multiplication Vector A | true     | 5       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |
            | Multiplication Vector B | true     | 0       | 10,-2,-3,-4,-5                      |
        When I multiply the vectors "Multiplication Vector A" and "Multiplication Vector B" and store the result in "Multiplication Result Vector C"
        Then the vector "Multiplication Result Vector C" should have the following values
    # TODO

    Scenario: Vector multiplication (dot product) 2D vs 2D
        Given I create the following vectors
            | Alias                   | IsCached | Columns | Values                              |
            | Multiplication Vector A | true     | 5       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |
            | Multiplication Vector B | true     | 5       | 1,2,3,4,5,6,7,8,9,10,-2,-3,-4,-5,-6 |
        When I multiply the vectors "Multiplication Vector A" and "Multiplication Vector B" and store the result in "Multiplication Result Vector C"
        Then the vector "Multiplication Result Vector C" should have the following values
    # TODO

    Scenario: Vector multiplication (cross product) 1D vs 2D
        Given I create the following vectors
            | Alias                   | IsCached | Columns | Values                              |
            | Multiplication Vector A | true     | 5       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |
            | Multiplication Vector B | true     | 0       | 10,-2,-3,-4,-5                      |
        When I multiply the vectors "Multiplication Vector A" and "Multiplication Vector B" and store the result in "Multiplication Result Vector C"
        Then the vector "Multiplication Result Vector C" should have the following values
    # TODO

    Scenario: Vector multiplication (cross product) 2D vs 2D
        Given I create the following vectors
            | Alias                   | IsCached | Columns | Values                              |
            | Multiplication Vector A | true     | 5       | 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15 |
            | Multiplication Vector B | true     | 5       | 1,2,3,4,5,6,7,8,9,10,-2,-3,-4,-5,-6 |
        When I multiply the vectors "Multiplication Vector A" and "Multiplication Vector B" and store the result in "Multiplication Result Vector C"
        Then the vector "Multiplication Result Vector C" should have the following values
# TODO

