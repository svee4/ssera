<script module lang="ts">
    

</script>


<script lang="ts">
	import Test from "$lib/components/Test.svelte";

	import { ImageApiHelper } from "$lib/ImageApiHelper";
	import { getEnumEntries, getEnumKeys } from "$lib/Utils";
	import type { Writable } from "svelte/store";
	import { queryParam } from "sveltekit-search-params";

    type Props = {
        tags: string[];
        tagSearch: string;
        eras: ImageApiHelper.Era[];
        members: ImageApiHelper.GroupMember[];

        orderBy: ImageApiHelper.OrderByType;
        sortBy: ImageApiHelper.SortType;
        pageSize: number;

        onSubmit: () => void;
    }

    // NOPE CANT USE IT AS AN OBJECT GOTTA BE VARIABLES
    let { 
        tags: tagsProp,
        tagSearch: tagSearchProp,
        eras: erasProp,
        members: membersProp,
        orderBy: orderByProp,
        sortBy: sortByProp,
        pageSize: pageSizeProp,
     }: Props = $props();

    const queryParamsOptions = {
        showDefaults: false,
        pushHistory: false,
    }

    let tagsStore = queryParam<string[]>("tags", {
        encode: v => v.join(","),
        decode: v => v?.split(",") ?? [],
        defaultValue: []
    }, queryParamsOptions);

    $effect(() => { tagsProp = $tagsStore });

    let tagSearchStore = queryParam<string>("tagSearch", {
        encode: v => v,
        decode: v => v ?? "",
        defaultValue: ""
    }, queryParamsOptions);

    $effect(() => { tagSearchProp = $tagSearchStore });

    let erasStore = queryParam<ImageApiHelper.Era[]>("eras", {
        encode: v => v.map(era => era.toString()).join(","),
        decode: v => v?.split(",").map(era => ImageApiHelper.Era[era as keyof typeof ImageApiHelper.Era]) ?? [],
        defaultValue: []
    }, queryParamsOptions);

    $effect(() => { erasProp = $erasStore });

    let membersStore = queryParam<ImageApiHelper.GroupMember[]>("members", {
        encode: v => v.map(member => member.toString()).join(","),
        decode: v => v?.split(",").map(era => ImageApiHelper.GroupMember[era as keyof typeof ImageApiHelper.GroupMember]) ?? [],
        defaultValue: []
    }, queryParamsOptions);

    $effect(() => { membersProp = $membersStore });

    let orderByStore = queryParam<ImageApiHelper.OrderByType>("orderBy", {
        encode: v => ImageApiHelper.OrderByType[v],
        decode: v => v ? ImageApiHelper.OrderByType[v as keyof typeof ImageApiHelper.OrderByType] : ImageApiHelper.OrderByType.Date,
        defaultValue: ImageApiHelper.OrderByType.Date
    }, queryParamsOptions);

    $effect(() => { orderByProp = $orderByStore });

    let sortStore = queryParam<ImageApiHelper.SortType>("sort", {
        encode: v => v.toString(),
        decode: v => v ? ImageApiHelper.SortType[v as keyof typeof ImageApiHelper.SortType] : ImageApiHelper.SortType.Descending,
        defaultValue: ImageApiHelper.SortType.Descending
    }, queryParamsOptions);

    $effect(() => { sortByProp = $sortStore });

    let pageSizeStore = queryParam<number>("pageSize", {
        encode: v => v.toString(),
        decode: v => v ? parseInt(v) : 50,
        defaultValue: 50
    }, queryParamsOptions);

    $effect(() => { pageSizeProp = $pageSizeStore });

    const preventDefault = <T extends Event>(f: (ev: T) => void) => 
        (ev: T) => {
            ev.preventDefault();
            f(ev);
        };

    const checkboxBindingSetter = <T, TStore extends Writable<T[]>>(value: T, store: TStore): (checked: boolean) => void => 
        (checked: boolean) => checked 
            ? store.update(arr => [value, ...arr]) 
            : erasStore.update(arr => arr.filter(v => v !== value))
    
    const submit = () => {}
</script>

<form id="form" onsubmit={preventDefault(submit)}>
    <h3>Filter</h3>
    <div id="filters-container">
        <div class="double-container">
            <div class="filter-item">
                <label for="orderby">Order by</label>
                <select id="orderby" bind:value={$orderByStore}>
                    {#each getEnumEntries(ImageApiHelper.OrderByType) as [name, value]}
                    <option value={value}>{name}</option>
                    {/each}
                </select>
            </div>

            <div class="filter-item">
                <label for="sort">Sort</label>
                <select id="sort" bind:value={$sortStore}>
                    {#each getEnumEntries(ImageApiHelper.SortType) as [name, value]}
                    <option value={value}>{name}</option>
                    {/each}
                </select>
            </div>
        </div>

        <div class="filter-item">
            <fieldset>
                <legend>Eras</legend>
                <div class="fieldset">
                    {#each getEnumEntries<ImageApiHelper.Era>(ImageApiHelper.Era) as [name, value]}
                    <div>
                        <input type="checkbox" id="era-{value}" bind:checked={
                            () => $erasStore.includes(value),
                            checkboxBindingSetter(value, erasStore)
                        } />
                        <label for="era-{value}">{ImageApiHelper.EraToHuman(value)}</label>
                    </div>
                    {/each}
                </div>
            </fieldset>
        </div>

        <div class="filter-item">
            <fieldset>
                <legend>Members</legend>
                <div class="fieldset">
                    {#each getEnumEntries<ImageApiHelper.GroupMember>(ImageApiHelper.GroupMember) as [name, value]}
                    <div>
                        <input type="checkbox" id="member-{value}" bind:checked={
                            () => $membersStore.includes(value),
                            checkboxBindingSetter(value, membersStore)
                        } />
                        <label for="member-{value}">{name}</label>
                    </div>
                    {/each}
                </div>
            </fieldset>
        </div>

        <div class="filter-item">
            <label for="pagesize">Results per page</label>
            <input type="number" min="10" max="500" inputmode="numeric" bind:value={$pageSizeStore} />
        </div>
    </div>
</form>

<style>
    #filters-container {
        display: flex;
        gap: 12px;
        align-items: center;
        flex-wrap: wrap;
    }

    .double-container {
        display: flex;
        flex-direction: column;
        gap: 6px;
    }

    .filter-item {
        display: flex;
        flex-direction: column;
    }

    legend {
        margin-left: 4px;
    }

    .fieldset {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 6px;
        padding: 6px;
    }
</style>