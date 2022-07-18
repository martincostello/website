// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Generator } from './Generator';

export class GuidGenerator extends Generator {
    private format: HTMLInputElement;
    private text: HTMLInputElement;
    private uppercase: HTMLInputElement;

    public constructor() {
        super();
    }

    public override initialize() {
        this.endpoint = this.getEndpoint('api-guid');

        if (this.endpoint) {
            this.format = document.getElementById(
                'guid-format'
            ) as HTMLInputElement;
            this.uppercase = document.getElementById(
                'guid-uppercase'
            ) as HTMLInputElement;

            const button = document.getElementById('generate-guid');
            button.addEventListener('click', async (event) => {
                event.preventDefault();
                await this.generateGuid();
            });

            this.text = document.getElementById(
                'text-guid'
            ) as HTMLInputElement;
        }
    }

    private async generateGuid(): Promise<void> {
        const formatValue = encodeURIComponent(this.format.value);
        const uppercaseValue = this.uppercase.checked;

        const uri = `${this.endpoint}?format=${formatValue}&uppercase=${uppercaseValue}`;

        const response = await this.getJson(uri);

        this.text.value = response.guid;
        this.text.setAttribute('value', response.guid);
    }
}
