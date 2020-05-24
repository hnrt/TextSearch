package com.hideakin.textsearch.index.converter;

import java.util.HashMap;
import java.util.Map;

import org.springframework.core.convert.converter.Converter;

import com.hideakin.textsearch.index.data.SearchOptions;

public class StringToSearchOptionsConverter implements Converter<String,SearchOptions> {

	private Map<String,String> map;

	public StringToSearchOptionsConverter() {
		map = new HashMap<String,String>();
		for (SearchOptions option : SearchOptions.values()) {
			map.put(option.name().toUpperCase(), option.name());
		}
	}

	@Override
	public SearchOptions convert(String source) {
		source = map.get(source.toUpperCase());
		if (source != null) {
			return SearchOptions.valueOf(source);
		} else {
			return SearchOptions.Exact;
		}
	}

}
