// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { describe, expect, test } from '@jest/globals';
import { Tools } from './Tools';

describe('Tools', () => {
    test('can be created', () => {
        const tools = new Tools();
        expect(tools).not.toBeNull();
    });
});
