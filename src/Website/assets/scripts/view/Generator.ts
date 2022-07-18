// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

export abstract class Generator {
    protected endpoint: string;

    protected constructor() {}

    public abstract initialize(): void;

    protected getEndpoint(name: string): string | null {
        const link = document.querySelector(`link[name='${name}']`);

        let endpoint: string | null = null;

        if (link) {
            endpoint = link.getAttribute('href');
        }

        return endpoint;
    }

    protected async getJson(uri: string): Promise<any> {
        const response = await fetch(uri);
        return await response.json();
    }

    protected async postJson(uri: string, payload: any): Promise<any> {
        const headers = new Headers();
        headers.set('Accept', 'application/json');
        headers.set('Content-Type', 'application/json');

        const init = {
            headers,
            method: 'POST',
            body: JSON.stringify(payload),
        };

        const response = await fetch(uri, init);
        return await response.json();
    }
}
