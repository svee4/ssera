import { ApiHelper } from "$lib/ApiHelper";

export module EventsApiHelper {

	// objects are organized so that the key is the api value and the value is the human readable value
	
	export const ApiRoute = `${ApiHelper.ApiDomain}/api/events` as const;

	export type ApiEvent = {
		date: string,
		type: keyof typeof AllEventTypes,
		title?: string,
		link?: string
	};
	
	export type ApiResponse = {
		events: ApiEvent[],
		lastUpdate?: string,
	}

	export type ApiQuery = {
		orderBy?: OrderByType,
		sort?: SortType
		eventTypes?: EventType[]
		search?: string
	}

	export const AllEventTypes = {
		"TeasersMV": "Teasers/MV",
		"Performance": "Performance",
		"MusicShows": "Music Shows",
		"BehindTheScenes": "Behind The Scenes",
		"Interview": "Interview",
		"Variety": "Variety",
		"Reality": "Reality",
		"CF": "CF",
		"Misc": "Miscellaneous",
		"MubankPresident": "Mubank President",
		"WeverseLive": "Weverse Live",
	} as const;

	export type EventType = typeof AllEventTypes[keyof typeof AllEventTypes];

	export const AllOrderByTypes = {
		"Date": "Date",
		"Type": "Type",
		"Title": "Title",
	} as const;

	export type OrderByType = typeof AllOrderByTypes[keyof typeof AllOrderByTypes];

	export const AllSortTypes = {
		"Ascending": "Ascending",
		"Descending": "Descending",
	} as const;

	export type SortType = typeof AllSortTypes[keyof typeof AllSortTypes];

	// manually synced from spreadsheet, colors are mixed with white for readability
	// TODO: manually lighten colors to look better and readable
	export const TypeColors = {
		"TeasersMV": "#ff00ff",
		"Performance": "#ff9900",
		"MusicShows": "#d0e0e3",
		"BehindTheScenes": "#0000ff",
		"Interview": "#00ff00",
		"Variety": "#9900ff",
		"Reality": "#ffff00",
		"CF": "#ffd966",
		"Misc": "#4a86e8",
		"MubankPresident": "#c27ba0",
		"WeverseLive": "#0be6c1",
	} as const;
}
