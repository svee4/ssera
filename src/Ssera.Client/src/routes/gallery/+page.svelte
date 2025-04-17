<script lang="ts">
	import { ImageApiHelper } from "$lib/ImageApiHelper";
	import { onMount } from "svelte";
	import type { Entry } from "./GalleryView.svelte";
	import GalleryView from "./GalleryView.svelte";
	import Filters from "./Filters.svelte";
	import PagedGalleryView from "./PagedGalleryView.svelte";
	import { queryParam } from "sveltekit-search-params";
	import { get } from "svelte/store";
	import type { ApiHelper } from "$lib/ApiHelper";
	import { json } from "@sveltejs/kit";

    type Filters = {
        tags: string[];
        tagSearch: string;
        eras: ImageApiHelper.Era[];
        members: ImageApiHelper.GroupMember[];

        orderBy: ImageApiHelper.OrderByType;
        sort: ImageApiHelper.SortType;
        pageSize: number;
    }
    
    let filters: Filters = $state({
        tags: [],
        tagSearch: "",
        eras: [],
        members: [],

        orderBy: 0,
        sort: 0,
        pageSize: 0,
    });

    type Error = {
        status: number;
        message: string;
        requestTraceId: string;
        activityTraceId: string;
    }

    let loading = $state(true);
    let error: Error | undefined = $state(undefined);
    let entries: Entry[] = $state([]);

    let pageStore = queryParam<number>("page", {
        encode: v => v.toString(), 
        decode: v => parseInt(v ?? "1"),
        defaultValue: 1
    }, {
        showDefaults: true,
        pushHistory: false,
        sort: true
    });
    
    let pageSize = $derived(filters.pageSize);

    let totalResults = $state(0);
    let maxPage = $derived(Math.ceil(totalResults / pageSize));

    const fetchEntries = async () => {
        loading = true;

        let q = new URLSearchParams();
        
        for (const tag in filters.tags) {
            q.append("tags", tag);
        }

        if (filters.tagSearch) {
            q.append("tagSearch", filters.tagSearch);
        }

        for (const era of filters.eras) {
            q.append("eras", ImageApiHelper.Era[era]);
        }

        for (const mem of filters.members) {
            q.append("members", ImageApiHelper.GroupMember[mem]);
        }

        q.append("orderBy", ImageApiHelper.OrderByType[filters.orderBy]);
        q.append("sort", ImageApiHelper.SortType[filters.sort]);
        q.append("pageSize", filters.pageSize.toString());
        q.append("page", get(pageStore).toString());

        const response = await fetch(ImageApiHelper.ApiRoute + "?" + q.toString());

        if (!response.ok) {
            const json = await response.json() as ApiHelper.ProblemDetails;
            console.error(json);

            error = {
                status: json.status,
                message: `${json.title}: ${json.detail}`,
                activityTraceId: json.activityTraceId,
                requestTraceId: json.requestTraceId
            };

            loading = false;
            return;
        }
        
        error = undefined;

        const data = (await response.json() as ImageApiHelper.GetResponse);

        entries = data.results
            .map(res => {
                let tags = res.tags;
                if (res.era) tags = [res.era, ...tags];
                return { id: res.id, tags, date: new Date(res.date) };
            });

        totalResults = data.totalResults;

        loading = false;
    }

    const applyNewFilters = () => {
        if (!loading) {
            fetchEntries();
        }
    }

    // cursed but works for now
    pageStore.subscribe(() => applyNewFilters());

    onMount(() => fetchEntries());
</script>

<div id="page">
    <!-- nope, no fucking spread here -->
    <Filters
        bind:tags={filters.tags}
        bind:tagSearch={filters.tagSearch}
        bind:eras={filters.eras}
        bind:members={filters.members}
        bind:orderBy={filters.orderBy}
        bind:sort={filters.sort}
        bind:pageSize={filters.pageSize}
        onSubmit={applyNewFilters} 
    />

    {#if error !== undefined}
    <div>
        <p>Error: <span>({error.status})</span> <span>{error.message}</span></p>
        <p>Request trace id: <span>{error.requestTraceId}</span></p>
        <p>Activity trace id: <span>{error.activityTraceId}</span></p>
    </div>
    {/if}

    {#if loading}
    
    <p>Loading...</p>

    {:else}

    <PagedGalleryView
        entries={entries} 
        totalResults={totalResults} 
        bind:page={$pageStore} 
        pageSize={pageSize} 
        maxPage={maxPage} 
    />

    {/if}
</div>

<style>
    #page {
        display: flex;
        flex-direction: column;
        gap: 12px;
    }
</style>