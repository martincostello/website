// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Generator } from './Generator';

export class HashGenerator extends Generator {
    private algorithm: HTMLInputElement;
    private format: HTMLInputElement;
    private plaintext: HTMLInputElement;
    private textContainer: HTMLElement;
    private text: HTMLInputElement;

    public constructor() {
        super();
    }

    public initialize(): void {
        this.endpoint = this.getEndpoint('api-hash');

        if (this.endpoint) {
            this.algorithm = document.getElementById(
                'hash-algorithm'
            ) as HTMLInputElement;
            this.format = document.getElementById(
                'hash-format'
            ) as HTMLInputElement;
            this.plaintext = document.getElementById(
                'hash-plaintext'
            ) as HTMLInputElement;

            const button = document.getElementById('generate-hash');
            button.addEventListener('click', async (event) => {
                event.preventDefault();
                await this.generate();
            });

            const copy = document.getElementById('copy-hash');
            copy.addEventListener('click', (event) => {
                event.preventDefault();
            });

            this.textContainer = document.getElementById('hash-container');
            this.text = document.getElementById(
                'text-hash'
            ) as HTMLInputElement;
        }
    }

    private async generate(): Promise<void> {
        const payload = {
            algorithm: this.algorithm.value || 'sha1',
            format: this.format.value || 'base64',
            plaintext: this.plaintext.value || '',
        };

        const response = await this.postJson(this.endpoint, payload);

        this.text.value = response.hash;
        this.text.setAttribute('value', response.hash);
        this.textContainer.classList.remove('d-none');
    }
}
