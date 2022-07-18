// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Generator } from './Generator';

export class MachineKeyGenerator extends Generator {
    private decryptionAlgorithm: HTMLInputElement;
    private validationAlgorithm: HTMLInputElement;
    private textContainer: HTMLElement;
    private text: HTMLInputElement;

    public constructor() {
        super();
    }

    public initialize(): void {
        this.endpoint = this.getEndpoint('api-machine-key');

        if (this.endpoint) {
            this.decryptionAlgorithm = document.getElementById(
                'key-decryption-algorithm'
            ) as HTMLInputElement;
            this.validationAlgorithm = document.getElementById(
                'key-validation-algorithm'
            ) as HTMLInputElement;

            const button = document.getElementById('generate-machine-key');
            button.addEventListener('click', async (event) => {
                event.preventDefault();
                await this.generate();
            });

            const copy = document.getElementById('copy-machine-key');
            copy.addEventListener('click', (event) => {
                event.preventDefault();
            });

            this.textContainer = document.getElementById(
                'machine-key-container'
            );
            this.text = document.getElementById(
                'machine-key-xml'
            ) as HTMLInputElement;
        }
    }

    private async generate(): Promise<void> {
        const query = `decryptionAlgorithm=${this.decryptionAlgorithm.value}&validationAlgorithm=${this.validationAlgorithm.value}`;
        const uri = `${this.endpoint}?${query}`;

        const response = await this.getJson(uri);

        this.text.textContent = response.machineKeyXml;
        this.text.setAttribute('value', response.machineKeyXml);
        this.textContainer.classList.remove('d-none');
    }
}
