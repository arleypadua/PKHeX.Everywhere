import { DateTime } from "luxon";
import siteConfig from "./_data/site.js";

export default function(eleventyConfig) {
    const config = siteConfig();
    eleventyConfig.addPassthroughCopy("css");
    eleventyConfig.addPassthroughCopy("font");
    eleventyConfig.addPassthroughCopy("favicon.svg");
    
    eleventyConfig.addFilter("readableDate", (dateObj) => {
        return DateTime.fromJSDate(dateObj).toFormat("MMMM dd, yyyy");
    });
    
    eleventyConfig.addFilter("withBaseUrl", (url) => {
        const urlWithoutSlash = url[0] === '/'
            ? url.replace('/', '')
            : url
        
        return `${config.baseUrl}${urlWithoutSlash}`;
    })

    eleventyConfig.addCollection("posts", function (collectionApi) {
        return collectionApi.getFilteredByGlob("posts/**/*.md").reverse();
    });

    eleventyConfig.addCollection("tagList", function (collectionApi) {
        const tagSet = new Set();
        collectionApi.getAll().forEach(item => {
            (item.data.tags || []).forEach(tag => tagSet.add(tag));
        });
        return [...tagSet];
    });
    
    // Order matters, put this at the top of your configuration file.
    eleventyConfig.setOutputDirectory("../wwwroot/blog");
};

