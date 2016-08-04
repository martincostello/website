martinCostello = {
    website: {
        branch: $("meta[name='x-site-branch']").attr("content"),
        revision: $("meta[name='x-site-revision']").attr("content"),
        track: function (category, action, label, value, fields) {
            if ("ga" in window) {
                ga("send", "event", category, action, label, value, fields);
            }
        }
    }
};
