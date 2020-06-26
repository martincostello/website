// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

describe("Given the namespaces are defined", () => {

    it("then martinCostello is not null", () => {
        expect(martinCostello).not.toBeNull();
    });

    it("then martinCostello.website is not null", () => {
        expect(martinCostello.website).not.toBeNull();
    });

    it("then martinCostello.website.tools is not null", () => {
        expect(martinCostello.website.tools).not.toBeNull();
    });
});

describe("Google Analytics", () => {

    describe("Given ga is not defined", () => {

        let analytics: UniversalAnalytics.ga;

        beforeEach(() => {
            analytics = ga;
            ga = null;
        });

        afterEach(() => {
            ga = analytics;
        });

        it("then an event is not published", () => {

            const category = "category";
            const action = "action";
            const label = "label";

            const result = martinCostello.website.Tracking.track(category, action, label);

            expect(result).toBe(false);
        });
    });

    describe("Given ga is defined", () => {

        beforeEach(() => {
            const spy = jasmine.createSpy("ga");
            ga = (spy as any) as UniversalAnalytics.ga;
        });

        afterEach(() => {
            ga = null;
        });

        it("then an event is published", () => {

            const category = "category";
            const action = "action";
            const label = "label";

            const result = martinCostello.website.Tracking.track(category, action, label);

            expect(result).toBe(true);
            expect(ga).toHaveBeenCalledWith("send", jasmine.objectContaining({
                hitType: "event",
                eventCategory: category,
                eventAction: action,
                eventLabel: label
            }));
        });
    });
});

describe("Debugging", () => {

    describe("Given meta tags containing the site version", () => {

        beforeAll(() => {
            $("head").append("<meta name='x-site-branch'] content='main' />");
            $("head").append("<meta name='x-site-revision'] content='012345ab' />");
        });

        it("then the branch is correct", () => {
            expect(martinCostello.website.Debug.branch()).toBe("main");
        });

        it("then the revision is correct", () => {
            expect(martinCostello.website.Debug.revision()).toBe("012345ab");
        });
    });

    describe("When logging", () => {

        beforeEach(() => {
            spyOn(console, "log");
        });

        it("then a message is logged", () => {
            martinCostello.website.Debug.log("2 + 2 = ", 2 + 2);
            expect(console.log).toHaveBeenCalledWith("2 + 2 = ", 4);
        });
    });
});
