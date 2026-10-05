import { describe, expect, it } from 'vitest'
import { pageItems } from './pageItems'

describe('pageItems', () => {
  it.each([
    [1, 1, [1]],
    [3, 7, [1, 2, 3, 4, 5, 6, 7]],
    [1, 26, [1, 2, 3, 4, 5, 'gap', 26]],
    [6, 26, [1, 'gap', 5, 6, 7, 'gap', 26]],
    [26, 26, [1, 'gap', 22, 23, 24, 25, 26]],
    // A gap would only hide page 2 here, so page 2 is shown instead.
    [4, 26, [1, 2, 3, 4, 5, 'gap', 26]],
  ])('page %i of %i', (current, total, expected) => {
    expect(pageItems(current, total)).toEqual(expected)
  })
})
