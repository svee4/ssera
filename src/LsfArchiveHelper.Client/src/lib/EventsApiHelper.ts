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
}
