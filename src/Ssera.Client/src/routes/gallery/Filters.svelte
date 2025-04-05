<script module lang="ts">
</script>

<script lang="ts">
	import Test from "$lib/components/Test.svelte";

	import { ImageApiHelper } from "$lib/ImageApiHelper";
	import { getEnumEntries, getEnumKeys } from "$lib/Utils";
	import { get, type Writable } from "svelte/store";
	import { queryParam } from "sveltekit-search-params";
	import Page from "../+page.svelte";

	type Props = {
		tags: string[];
		tagSearch: string;
		eras: ImageApiHelper.Era[];
		members: ImageApiHelper.GroupMember[];

		orderBy: ImageApiHelper.OrderByType;
		sort: ImageApiHelper.SortType;
		pageSize: number;

		onSubmit: () => void;
	};

	// NOPE CANT USE IT AS AN OBJECT GOTTA BE VARIABLES
	let {
		tags: tagsProp = $bindable(),
		tagSearch: tagSearchProp = $bindable(),
		eras: erasProp = $bindable(),
		members: membersProp = $bindable(),
		orderBy: orderByProp = $bindable(),
		sort: sortProp = $bindable(),
		pageSize: pageSizeProp = $bindable(),
		onSubmit: onSubmitProp,
	}: Props = $props();

    let dirty = $state(false);

	const queryParamsOptions = {
		showDefaults: false,
		pushHistory: false,
		sort: true
	};

	let tagsStore = queryParam<string[]>(
		"tags",
		{
			encode: (v) => v.join(",") || undefined,
			decode: (v) => v?.split(",") ?? [],
			defaultValue: [],
		},
		queryParamsOptions,
	);

	let tagSearchStore = queryParam<string>(
		"tagSearch",
		{
			encode: (v) => v || undefined,
			decode: (v) => v ?? "",
			defaultValue: "",
		},
		queryParamsOptions,
	);

	let erasStore = queryParam<ImageApiHelper.Era[]>(
		"eras",
		{
			encode: (v) => v.map(era => ImageApiHelper.Era[era]).filter(e => e !== undefined).join(",") || undefined,
			decode: (v) => v?.split(",")
                    .map(v => ImageApiHelper.Era[v as keyof typeof ImageApiHelper.Era])
                    .filter(era => era !== undefined)
					?? [],
			defaultValue: [],
		},
		queryParamsOptions,
	);

	let membersStore = queryParam<ImageApiHelper.GroupMember[]>(
		"members",
		{
			encode: (v) => v.map(mem => ImageApiHelper.GroupMember[mem]).filter(mem => mem !== undefined).join(",") || undefined,
			decode: (v) => v?.split(",")
                    .map(v => ImageApiHelper.GroupMember[v as keyof typeof ImageApiHelper.GroupMember])
                    .filter(era => era !== undefined) 
					?? [],
			defaultValue: [],
		},
		queryParamsOptions,
	);

	let orderByStore = queryParam<ImageApiHelper.OrderByType>(
		"orderBy",
		{
			encode: (v) => ImageApiHelper.OrderByType[v],
			decode: (v) => v
					? ImageApiHelper.OrderByType[v as keyof typeof ImageApiHelper.OrderByType]
					: ImageApiHelper.OrderByType.Date,
			defaultValue: ImageApiHelper.OrderByType.Date,
		},
		queryParamsOptions,
	);

	let sortStore = queryParam<ImageApiHelper.SortType>(
		"sort",
		{
			encode: (v) => ImageApiHelper.SortType[v],
			decode: (v) => v
					? ImageApiHelper.SortType[v as keyof typeof ImageApiHelper.SortType]
					: ImageApiHelper.SortType.Descending,
			defaultValue: ImageApiHelper.SortType.Descending,
		},
		queryParamsOptions,
	);

	let pageSizeStore = queryParam<number>(
		"pageSize",
		{
			encode: (v) => v.toString(),
			decode: (v) => v && !isNaN(parseInt(v)) ? parseInt(v) : 50,
			defaultValue: 50,
		},
		{ 
			...queryParamsOptions,
			showDefaults: true
		 },
	);

	const getPrev = () => {
		return {
			tags: get(tagsStore),
			tagSearch: get(tagSearchStore),
			eras: get(erasStore),
			members: get(membersStore),
			orderBy: get(orderByStore),
			sort: get(sortStore),
			pageSize: get(pageSizeStore),
		};
	}

	let prev = $state(getPrev());

	$effect(() => {
		tagsProp = $tagsStore;
		tagSearchProp = $tagSearchStore;
		erasProp = $erasStore;
		membersProp = $membersStore;
		orderByProp = $orderByStore;
		sortProp = $sortStore
		pageSizeProp = $pageSizeStore;

		dirty = 
			!arrayEquals(prev.tags, $tagsStore)
			|| prev.tagSearch !== $tagSearchStore
			|| !arrayEquals(prev.eras, $erasStore)
			|| !arrayEquals(prev.members, $membersStore)
			|| prev.orderBy !== $orderByStore
			|| prev.sort !== $sortStore
			|| prev.pageSize !== $pageSizeStore;
	});

	const arrayEquals = <T>(arr1: T[], arr2: T[], comparer: ((p1: T, p2: T) => boolean) | undefined = undefined) => {
		if (arr1.length !== arr2.length) {
			return false;
		}

		if (comparer === undefined) {
			comparer = (p1, p2) => p1 === p2;
		}

		for (let i = 0; i < arr1.length; i++) {
			if (!comparer(arr1[i], arr2[i])) {
				return false;
			}
		}

		return true;
	}

	const preventDefault =
		<T extends Event>(f: (ev: T) => void) =>
		(ev: T) => {
			ev.preventDefault();
			f(ev);
		};

	const checkboxBindingSetter =
		<T, TStore extends Writable<T[]>>(value: T, store: TStore): ((checked: boolean) => void) =>
		(checked: boolean) => {
            checked
                ? store.update(arr => [...arr, value].sort())
                : store.update(arr => arr.filter((v) => v !== value).sort());
        }

	const submit = () => {
		prev = getPrev(); 
		onSubmitProp();
	} 
</script>

<form id="form" onsubmit={preventDefault(submit)}>
	<h3>Filter</h3>
	<div id="filters-container">

		<div class="double-container">
			<div class="filter-item">
				<label for="orderby">Order by</label>
				<select id="orderby" bind:value={$orderByStore}>
					{#each getEnumEntries(ImageApiHelper.OrderByType) as [name, value]}
						<option {value}>{name}</option>
					{/each}
				</select>
			</div>

			<div class="filter-item">
				<label for="sort">Sort</label>
				<select id="sort" bind:value={$sortStore}>
					{#each getEnumEntries(ImageApiHelper.SortType) as [name, value]}
						<option {value}>{name}</option>
					{/each}
				</select>
			</div>
		</div>

		<div class="double-container">

			<div class="filter-item">
				<label for="tagSearch">Search tag</label>
				<input type="text" id="tagSearch" bind:value={$tagSearchStore} />
			</div>

			<div class="filter-item">
				<label for="pagesize">Results per page</label>
				<input
					id="pagesize"
					type="number"
					min="10"
					max="500"
					inputmode="numeric"
					bind:value={$pageSizeStore}
				/>
			</div>
		</div>

		<div class="filter-item">
			<fieldset>
				<legend>Eras</legend>
				<div class="fieldset">
					{#each getEnumEntries<ImageApiHelper.Era>(ImageApiHelper.Era) as [name, value]}
						<div>
							<input
								type="checkbox"
								id="era-{value}"
								bind:checked={
									() => $erasStore.includes(value),
									checkboxBindingSetter(value, erasStore)
								}
							/>
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
							<input
								type="checkbox"
								id="member-{value}"
								bind:checked={
									() => $membersStore.includes(value),
									checkboxBindingSetter(value, membersStore)
								}
							/>
							<label for="member-{value}">{name}</label>
						</div>
					{/each}
				</div>
			</fieldset>
		</div>

		<div class="double-container">
			<div class="filter-item">
				<button type="submit">Apply</button>
			</div>
            <div class="filter-item">
                <p style="{dirty ? "" : "visibility: hidden"}">Unapplied changes</p>
            </div>
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
