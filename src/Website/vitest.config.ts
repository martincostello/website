import { defineConfig } from 'vitest/config';

export default defineConfig({
    test: {
        clearMocks: true,
        coverage: {
            enabled: true,
            provider: 'v8',
            include: ['assets/scripts/**/*.ts'],
            reporter: ['html', 'text'],
        },
        reporters: ['default', 'github-actions'],
    },
});
