// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { GuidGenerator } from './GuidGenerator';
import { HashGenerator } from './HashGenerator';
import { MachineKeyGenerator } from './MachineKeyGenerator';

export class Tools {
    private readonly guid: GuidGenerator;
    private readonly hash: HashGenerator;
    private readonly key: MachineKeyGenerator;

    public constructor() {
        this.guid = new GuidGenerator();
        this.hash = new HashGenerator();
        this.key = new MachineKeyGenerator();
    }

    public initialize(): void {
        this.guid.initialize();
        this.hash.initialize();
        this.key.initialize();

        if ('ClipboardJS' in window) {
            const selector = '.copy-button';
            const copyButton = document.querySelector(selector);

            const clipboard: any = window['ClipboardJS' as any];
            new clipboard(selector);
            copyButton.addEventListener('click', (event) => {
                event.preventDefault();
            });
        }
    }
}
